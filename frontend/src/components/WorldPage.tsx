import {
	Box,
	Grid,
	GridItem,
	Heading,
	Spinner,
	Text,
	VStack,
	useToast,
} from "@chakra-ui/react";
import { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { ethers, ZeroAddress } from "ethers";
import { CONTRACTS } from "../constants/addresses";

const SUBGRAPH_URL =
	"http://localhost:8000/subgraphs/name/monolyth/game-subgraph";

const GRID_SIZE = 10;

type Plot = {
	id: string;
	x: number;
	y: number;
	price: string;
	owner: {
		id: string;
	};
};

type Player = {
	id: string;
	username: string;
};

type Resource = {
	id: string;
	r1: string;
	r2: string;
	r3: string;
	r4: string;
	r5: string;
	plotPrice: string;
};

const WorldPage: React.FC = () => {
	const { id } = useParams<{ id: string }>();
	const toast = useToast();

	const [plotOwners, setPlotOwners] = useState<string[][] | null>(null);
	const [usernameMap, setUsernameMap] = useState<Record<string, string>>({});
	const [resource, setResource] = useState<Resource | null>(null);
	const [currentUser, setCurrentUser] = useState<string>("");
	const [loading, setLoading] = useState(true);

	const fetchWorldData = useCallback(
		async (playerOverride?: string) => {
			const player = playerOverride || currentUser;
			if (!id || !player) return;
			setLoading(true);

			try {
				const res = await fetch(SUBGRAPH_URL, {
					method: "POST",
					headers: { "Content-Type": "application/json" },
					body: JSON.stringify({
						query: `
						query GetWorldData($worldId: BigInt!, $playerId: String!) {
							gamePlots(where: { worldId: $worldId }) {
								id
								x
								y
								price
								owner { id }
							}
							playerNFTs {
								id
								username
							}
							playerResource(id: $playerId) {
								id
								r1
								r2
								r3
								r4
								r5
								plotPrice
							}
						}
					`,
						variables: {
							worldId: id,
							playerId: `${player.toLowerCase()}-${id}`,
						},
					}),
				});

				const json = await res.json();

				if (json.errors) {
					console.error("GraphQL errors:", json.errors);
					throw new Error("Subgraph query failed");
				}

				const plots: Plot[] = json.data.gamePlots;
				const players: Player[] = json.data.playerNFTs;
				const resourceData: Resource | null = json.data.playerResource;

				const ownerMap: string[][] = Array(GRID_SIZE)
					.fill(null)
					.map(() => Array(GRID_SIZE).fill(ZeroAddress.toLowerCase()));

				for (const plot of plots) {
					const { x, y, owner } = plot;
					if (x < GRID_SIZE && y < GRID_SIZE) {
						ownerMap[x][y] = owner.id.split("-")[0];
					}
				}

				const nameMap: Record<string, string> = {};
				for (const p of players) {
					nameMap[p.id.toLowerCase()] = p.username;
				}

				setUsernameMap(nameMap);
				setPlotOwners(ownerMap);
				setResource(resourceData ?? null);
			} catch (err) {
				toast({
					title: "Failed to load world data",
					description: "Check Subgraph connection",
					status: "error",
					duration: 5000,
					isClosable: true,
				});
				console.error("World fetch failed:", err);
			} finally {
				setLoading(false);
			}
		},
		[id, currentUser, toast], // ✅ include all referenced deps
	);

	useEffect(() => {
		if (!window.ethereum || !id) return;

		const getUser = async () => {
			const provider = new ethers.BrowserProvider(window.ethereum);
			const signer = await provider.getSigner();
			const address = await signer.getAddress();
			setCurrentUser(address.toLowerCase());
			await fetchWorldData(address.toLowerCase());
		};

		getUser().catch(console.error);
	}, [id, fetchWorldData]);

	const handleBuyPlot = async (x: number, y: number) => {
		if (!id || !currentUser) return;

		const toastId = toast({
			title: "Buying Plot...",
			description: `Buying plot (${x}, ${y})...`,
			status: "info",
			duration: 5000,
			isClosable: true,
		});

		try {
			const response = await fetch("http://localhost:3001/buy-plot", {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
				body: JSON.stringify({
					worldId: id,
					player: currentUser,
					x,
					y,
				}),
			});

			const json = await response.json();

			if (!response.ok) {
				throw new Error(json.error || "Plot purchase failed");
			}

			toast.update(toastId, {
				title: "Success 🎉",
				description: json.message,
				status: "success",
				duration: 5000,
				isClosable: true,
			});

			await fetchWorldData(currentUser); // refresh UI
		} catch (err: unknown) {
			console.error("Buy plot error", err);
			toast.update(toastId, {
				title: "Transaction Failed",
				description: (err as Error).message || "Unknown error",
				status: "error",
				duration: 5000,
				isClosable: true,
			});
		}
	};

	const renderPlot = (x: number, y: number, owner: string | null) => {
		const isLoading = loading || owner === null;
		const isUnowned = owner === ZeroAddress.toLowerCase();
		const isMine = owner === currentUser.toLowerCase();

		let bgColor = "gray.300";
		let content: React.ReactNode = null;
		let cursor = "default";
		let title = "Loading...";

		if (isLoading) {
			content = <Spinner size="xs" />;
		} else if (isUnowned) {
			bgColor = "gray.100";
			content = "+";
			cursor = "pointer";
			title = `Buy plot (${x}, ${y})`;
		} else {
			bgColor = isMine ? "green.300" : "red.300";
			content = usernameMap[owner]?.slice(0, 2).toUpperCase() || "X";
			title = `Owned by ${usernameMap[owner] || owner.slice(0, 6)}`;
		}

		return (
			<GridItem
				key={`${x}-${y}`}
				w="40px"
				h="40px"
				bg={bgColor}
				border="1px solid"
				borderColor="gray.400"
				fontSize="xs"
				display="flex"
				alignItems="center"
				justifyContent="center"
				cursor={cursor}
				title={title}
				onClick={() => {
					if (isUnowned && !loading) {
						handleBuyPlot(x, y);
					}
				}}
			>
				{content}
			</GridItem>
		);
	};

	return (
		<Box borderWidth="1px" borderRadius="md" p={6}>
			<Heading size="md">🌍 World {id}</Heading>

			<Text mt={4} mb={2}>
				Resource Balance:
			</Text>
			<VStack align="start" spacing={1}>
				{resource ? (
					<>
						<Text>R1: {resource.r1}</Text>
						<Text>R2: {resource.r2}</Text>
						<Text>R3: {resource.r3}</Text>
						<Text>R4: {resource.r4}</Text>
						<Text>R5: {resource.r5}</Text>
					</>
				) : (
					<Text fontStyle="italic">Loading...</Text>
				)}
			</VStack>

			<Text mt={4}>Next Plot Price: {resource?.plotPrice ?? "..."}</Text>

			<Text mt={6} mb={2}>
				Plot Grid:
			</Text>
			<Grid templateColumns="repeat(10, 40px)" gap={1}>
				{Array.from({ length: GRID_SIZE }).map((_, x) =>
					Array.from({ length: GRID_SIZE }).map((_, y) =>
						renderPlot(x, y, plotOwners?.[x]?.[y] ?? null),
					),
				)}
			</Grid>
		</Box>
	);
};

export default WorldPage;
