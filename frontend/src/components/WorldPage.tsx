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
import GameRegistryABI from "../abis/GameRegistryABI.json";

const WorldPage: React.FC = () => {
	const { id } = useParams<{ id: string }>();
	const [resources, setResources] = useState<number[]>([0, 0, 0, 0, 0]);
	const [plotOwners, setPlotOwners] = useState<string[][]>(
		Array.from({ length: 10 }, () => Array(10).fill("")),
	);
	const [usernameMap, setUsernameMap] = useState<Record<string, string>>({});
	const [currentUser, setCurrentUser] = useState<string>("");
	const [price, setPrice] = useState("...");
	const toast = useToast();
	const [loading, setLoading] = useState(true);
	const [buyingPlot, setBuyingPlot] = useState<{ x: number; y: number } | null>(
		null,
	);

	const loadWorldData = useCallback(async () => {
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

			const registry = new ethers.Contract(
				CONTRACTS.gameRegistryAddress,
				GameRegistryABI.abi,
				provider,
			);

			const r = await game.getPlayerResources(id, address);
			setResources(r.map((n: BigNumberish) => Number.parseInt(n.toString())));

			const p = await game.getPlotPrice(id, address);
			setPrice(p.toString());

			const ownersFlat: string[] = await game.getAllPlotOwners(id);
			const owners: string[][] = [];
			const usernames: Record<string, string> = {};

			for (let x = 0; x < 10; x++) {
				owners[x] = [];
				for (let y = 0; y < 10; y++) {
					const index = x * 10 + y;
					const owner = ownersFlat[index].toLowerCase();
					owners[x][y] = owner;

					if (owner !== ZeroAddress.toLowerCase() && !usernames[owner]) {
						try {
							usernames[owner] = await registry.getUsername(owner);
						} catch {
							usernames[owner] = owner.slice(0, 6);
						}
					}
				}
			}

			setUsernameMap(usernames);
			setPlotOwners(owners);
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
	}, [id, toast]);

	useEffect(() => {
		loadWorldData();
	}, [loadWorldData]);

	const renderPlot = (x: number, y: number, owner: string) => {
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
					body: JSON.stringify({
						worldId: id,
						player: currentUser,
						x,
						y,
					}),
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
			} finally {
				setBuyingPlot(null);
			}
		};

		const bgColor = isUnowned ? "gray.100" : isMine ? "green.300" : "red.300";
		const isDisabled = !!buyingPlot && !(isBuyingThis || isUnowned);
		const content = isBuyingThis ? (
			<Spinner size="xs" />
		) : isUnowned ? (
			"+"
		) : (
			usernameMap[owner]?.slice(0, 2).toUpperCase() || "X"
		);

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
				flexDirection="column"
				cursor={isUnowned && !isDisabled ? "pointer" : "default"}
				onClick={isUnowned && !isDisabled ? handleBuy : undefined}
				title={
					isUnowned
						? `Buy plot (${x}, ${y})`
						: `Owned by ${usernameMap[owner] || owner.slice(0, 6)}`
				}
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
					<Text key={`resource-${amount}-${id}`}>
						R{idx + 1}: {amount}
					</Text>
				))}
			</VStack>
			<Text mt={4}>Next Plot Price: {price}</Text>
			<Text mt={6} mb={2}>
				Plot Grid:
			</Text>
			<Grid templateColumns="repeat(10, 40px)" gap={1}>
				{plotOwners.map((row, x) =>
					row.map((owner, y) => renderPlot(x, y, owner)),
				)}
			</Grid>
		</Box>
	);
};

export default WorldPage;
