import { Box, Container, Heading, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { ethers } from "ethers";
import { CONTRACTS } from "../constants/addresses";
import GameRegistryABI from "../abis/GameRegistryABI.json";
import JoinWorld from "./JoinWorld";

type Props = {
  account: string | null;
};

const Home: React.FC<Props> = ({ account }) => {
  const [username, setUsername] = useState("");

  useEffect(() => {
    const fetchUsername = async () => {
      if (!window.ethereum || !account) return;

      const provider = new ethers.BrowserProvider(window.ethereum);
      const contract = new ethers.Contract(CONTRACTS.gameRegistryAddress, GameRegistryABI.abi, provider);
      const name = await contract.getUsername(account);
      setUsername(name);
    };

    fetchUsername();
  }, [account]);

  return (
    <Container maxW="container.md" pt={10}>
      <Box borderWidth="1px" borderRadius="lg" p={5} mb={6}>
        <Heading size="md">🏡 Home</Heading>
        <Text mt={4}>Welcome, {username || "player"}!</Text>
      </Box>

      <JoinWorld />
    </Container>
  );
};

export default Home;
