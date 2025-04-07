import type React from "react";
import { useEffect, useState } from "react";
import {
	Box,
	Button,
	Heading,
	Select,
	VStack,
	useToast,
} from "@chakra-ui/react";
import { ethers } from "ethers";
import { useNavigate } from "react-router-dom";
import { CONTRACTS } from "../constants/addresses";
import GameEngineABI from "../abis/GameEngineABI.json";

declare global {
	interface Window {
		ethereum?: import("@metamask/providers").MetaMaskInpageProvider;
	}
}

const JoinWorld: React.FC = () => {
	const [signer, setSigner] = useState<ethers.Signer | null>(null);
	const [contract, setContract] = useState<ethers.Contract | null>(null);
	const [account, setAccount] = useState<string>("");
	const [worldId, setWorldId] = useState("1");
	const [hasJoined, setHasJoined] = useState(false);
	const [checking, setChecking] = useState(true);
	const toast = useToast();
	const navigate = useNavigate();

	useEffect(() => {
		const init = async () => {
			if (!window.ethereum) return;

			const browserProvider = new ethers.BrowserProvider(window.ethereum);
			const signer = await browserProvider.getSigner();
			const addr = await signer.getAddress();
			const c = new ethers.Contract(
				CONTRACTS.gameEngineAddress,
				GameEngineABI.abi,
				signer,
			);

			setSigner(signer);
			setContract(c);
			setAccount(addr);
		};

		init();
	}, []);

	useEffect(() => {
		if (contract && account && worldId) {
			fetchStatus();
		}
	}, [contract, account, worldId]);

	const fetchStatus = async () => {
		setChecking(true);
		try {
			if (!contract?.hasJoined) {
				throw new Error("❌ 'hasJoined' function is missing in GameEngine ABI");
			}

			const joined = await contract.hasJoined(account, worldId);
			setHasJoined(joined);
		} catch (err) {
			console.error("Error calling hasJoined():", err);
			toast({
				title: "Contract error",
				description: "Could not check world status. Check ABI & chain.",
				status: "error",
				duration: 4000,
				isClosable: true,
			});
		} finally {
			setChecking(false);
		}
	};

	const joinWorld = async () => {
		try {
			if (!contract || !signer) throw new Error("Wallet not connected");

			const tx = await contract.joinWorld(worldId, {
				value: ethers.parseEther("10"),
			});
			await tx.wait();

			toast({
				title: "World joined",
				description: `You joined world ${worldId}`,
				status: "success",
				duration: 4000,
				isClosable: true,
			});

			navigate(`/world/${worldId}`);
		} catch (err: unknown) {
			const errorMessage =
				err instanceof Error ? err.message : "An unknown error occurred";
			toast({
				title: "Join failed",
				description: errorMessage,
				status: "error",
				duration: 5000,
				isClosable: true,
			});
		}
	};

	return (
		<Box borderWidth="1px" borderRadius="md" p={6}>
			<VStack spacing={4} align="stretch">
				<Heading size="md">🌍 Join a World</Heading>

				<Select value={worldId} onChange={(e) => setWorldId(e.target.value)}>
					<option value="1">World 1</option>
					<option value="2">World 2</option>
					<option value="3">World 3</option>
				</Select>

				<Button
					colorScheme="teal"
					onClick={() => {
						if (hasJoined) {
							navigate(`/world/${worldId}`);
						} else {
							joinWorld();
						}
					}}
					isDisabled={!account || checking}
				>
					{hasJoined ? "Enter World" : "Join for 10 tHYDRA"}
				</Button>
			</VStack>
		</Box>
	);
};

export default JoinWorld;
