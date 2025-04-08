import type React from "react";
import { useState } from "react";
import {
	Box,
	Button,
	Heading,
	Input,
	VStack,
	useToast,
} from "@chakra-ui/react";
import { ethers } from "ethers";
import GameRegistryABI from "../abis/GameRegistryABI.json";
import { CONTRACTS } from "../constants/addresses";
import { useNavigate } from "react-router-dom";

declare global {
	interface Window {
		ethereum?: import("@metamask/providers").MetaMaskInpageProvider;
	}
}

type Props = {
	account: string | null;
	connect: () => void;
};

const Register: React.FC<Props> = ({ account, connect }) => {
	const [username, setUsername] = useState("");
	const [loading, setLoading] = useState(false);
	const [loginLoading, setLoginLoading] = useState(false);
	const toast = useToast();
	const navigate = useNavigate();

	const handleLogin = async () => {
		try {
			setLoginLoading(true);

			if (!window.ethereum) throw new Error("Wallet not found");

			const provider = new ethers.BrowserProvider(window.ethereum);
			const signer = await provider.getSigner();
			const address = await signer.getAddress();

			const contract = new ethers.Contract(
				CONTRACTS.gameRegistryAddress,
				GameRegistryABI.abi,
				provider,
			);
			const registered = await contract.isRegistered(address);

			if (registered) {
				navigate("/home");
			} else {
				toast({
					title: "Not Registered",
					description: "This wallet is not registered yet.",
					status: "warning",
					duration: 4000,
					isClosable: true,
				});
			}
		} catch (err: unknown) {
			toast({
				title: "Login Error",
				description: err instanceof Error ? err.message : "Login failed",
				status: "error",
				duration: 5000,
				isClosable: true,
			});
		} finally {
			setLoginLoading(false);
		}
	};

	const handleRegister = async () => {
		try {
			if (!window.ethereum) throw new Error("Wallet not found");
			if (!username) throw new Error("Username is required");

			setLoading(true);

			const provider = new ethers.BrowserProvider(window.ethereum);
			const signer = await provider.getSigner();
			const contract = new ethers.Contract(
				CONTRACTS.gameRegistryAddress,
				GameRegistryABI.abi,
				signer,
			);

			const tx = await contract.register(username);
			await tx.wait();

			toast({
				title: "Registration successful",
				status: "success",
				duration: 3000,
				isClosable: true,
			});

			navigate("/home");
		} catch (err: unknown) {
			toast({
				title: "Registration Error",
				description: err instanceof Error ? err.message : "Something went wrong",
				status: "error",
				duration: 5000,
				isClosable: true,
			});
		} finally {
			setLoading(false);
		}
	};

	if (!account) {
		return (
			<Button colorScheme="blue" onClick={connect}>
				Connect Wallet
			</Button>
		);
	}

	return (
		<Box maxW="sm" mx="auto" mt={20} p={8} borderWidth={1} borderRadius="lg">
			<VStack spacing={4}>
				<Heading size="md">Register Your Username</Heading>
				<Input
					placeholder="Enter unique username"
					value={username}
					onChange={(e) => setUsername(e.target.value)}
				/>
				<Button
					colorScheme="teal"
					onClick={handleRegister}
					isLoading={loading}
					width="100%"
				>
					Register
				</Button>
			</VStack>
			<VStack spacing={4} mt={6}>
				<Button
					onClick={handleLogin}
					isLoading={loginLoading}
					width="100%"
					variant="outline"
				>
					Login
				</Button>
			</VStack>
		</Box>
	);
};

export default Register;
