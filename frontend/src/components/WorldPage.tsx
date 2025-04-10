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
import { ethers, ZeroAddress, type BigNumberish } from "ethers";
import { CONTRACTS } from "../constants/addresses";
import GameEngineABI from "../abis/GameEngineABI.json";
import PlayerNFTABI from "../abis/PlayerNFTABI.json";

const WorldPage: React.FC = () => {
	const { id } = useParams<{ id: string }>();
	const [resources, setResources] = useState<number[]>([0, 0, 0, 0, 0]);
	const [plotOwners, setPlotOwners] = useState<string[][] | null>(null);
	const [usernameMap, setUsernameMap] = useState<Record<string, string>>({});
	const [currentUser, setCurrentUser] = useState<string>("");
	const [price, setPrice] = useState("...");
	const toast = useToast();
	const [loading, setLoading] = useState(true);
	const [buyingPlot, setBuyingPlot] = useState<{ x: number; y: number } | null>(
		null,
	);

	const loadWorldData = useCallback(
		async (onlyUpdateUnowned = false) => {
			if (!window.ethereum || !id) return;
			setLoading(true);

			try {
				const provider = new ethers.BrowserProvider(window.ethereum);
				const signer = await provider.getSigner();
				const address = await signer.getAddress();
				setCurrentUser(address);

				const game = new ethers.Contract(
					CONTRACTS.gameEngineAddress,
					GameEngineABI.abi,
					provider,
				);

				const playerNFT = new ethers.Contract(
					CONTRACTS.playerNFTAddress,
					PlayerNFTABI.abi,
					provider,
				);

				// Always update resources and price for current player
				const r = await game.getPlayerResources(id, address);
				setResources(r.map((n: BigNumberish) => Number.parseInt(n.toString())));

				const p = await game.getPlotPrice(id, address);
				setPrice(p.toString());

				const ownersFlat: string[] = await game.getAllPlotOwners(id);
				const updatedOwners: string[][] = plotOwners
					? [...plotOwners.map((row) => [...row])]
					: [];

				const usernames: Record<string, string> = {};

				for (let x = 0; x < 10; x++) {
					if (!updatedOwners[x]) updatedOwners[x] = [];

					for (let y = 0; y < 10; y++) {
						const index = x * 10 + y;
						const newOwner = ownersFlat[index].toLowerCase();

						const wasUnowned =
							plotOwners?.[x]?.[y] === ZeroAddress.toLowerCase();

						const shouldUpdate =
							!onlyUpdateUnowned || wasUnowned || !plotOwners?.[x]?.[y];

						if (shouldUpdate) {
							updatedOwners[x][y] = newOwner;

							if (
								newOwner !== ZeroAddress.toLowerCase() &&
								!usernames[newOwner]
							) {
								try {
									const name = await playerNFT.getUsername(newOwner);
									usernames[newOwner] = name;
								} catch {
									usernames[newOwner] = newOwner.slice(0, 6);
								}
							}
						}
					}
				}

				setPlotOwners(updatedOwners);
				setUsernameMap((prev) => ({ ...prev, ...usernames }));
			} catch (err) {
				toast({
					title: "Error loading world",
					description: "Check your connection or contract",
					status: "error",
					duration: 5000,
					isClosable: true,
				});
			} finally {
				setLoading(false);
			}
		},
		[id, toast],
	);

	useEffect(() => {
		loadWorldData();
	}, [loadWorldData]);

	const renderPlot = (x: number, y: number, owner: string | null) => {
		const isLoading = loading || owner === null;
		const isUnowned = owner === ZeroAddress.toLowerCase();
		const isMine = owner === currentUser.toLowerCase();
		const isBuyingThis = buyingPlot?.x === x && buyingPlot?.y === y;

		const handleBuy = async () => {
			if (!id || !currentUser) return;
			setBuyingPlot({ x, y });

			try {
				const response = await fetch("http://localhost:3001/buy-plot", {
					method: "POST",
					headers: { "Content-Type": "application/json" },
					body: JSON.stringify({ worldId: id, player: currentUser, x, y }),
				});

				const result = await response.json();

				if (response.ok) {
					toast({
						title: "✅ Plot Purchased",
						description: `Plot (${x}, ${y}) now owned by you`,
						status: "success",
						duration: 4000,
						isClosable: true,
					});
					await loadWorldData();
					setBuyingPlot(null);
				} else {
					throw new Error(result.error || "Something went wrong");
				}
			} catch (err: unknown) {
				toast({
					title: "❌ Purchase Failed",
					description: err instanceof Error ? err.message : "Unknown error",
					status: "error",
					duration: 5000,
					isClosable: true,
				});
				setBuyingPlot(null);
			}
		};

		let bgColor = "gray.300";
		let content: React.ReactNode = null;
		let cursor = "default";
		let title = "Loading...";

		if (isLoading) {
			bgColor = "gray.300";
			content = null;
		} else if (isBuyingThis) {
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
				onClick={!isLoading && isUnowned && !buyingPlot ? handleBuy : undefined}
				title={title}
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
				{resources.map((amount, idx) => (
					<Text key={`R${idx + 1}`}>
						R{idx + 1}: {amount}
					</Text>
				))}
			</VStack>
			<Text mt={4}>Next Plot Price: {price}</Text>
			<Text mt={6} mb={2}>
				Plot Grid:
			</Text>
			<Grid templateColumns="repeat(10, 40px)" gap={1}>
				{Array.from({ length: 10 }).map((_, x) =>
					Array.from({ length: 10 }).map((_, y) =>
						renderPlot(x, y, plotOwners?.[x]?.[y] ?? null),
					),
				)}
			</Grid>
		</Box>
	);
};

export default WorldPage;
