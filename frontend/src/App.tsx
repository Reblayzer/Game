import {
	BrowserRouter as Router,
	Routes,
	Route,
	Navigate,
} from "react-router-dom";
import { ChakraProvider, Flex, Heading } from "@chakra-ui/react";
import { useWallet } from "./hooks/useWallet";
import Register from "./components/Register";
import Home from "./components/Home";
import WorldPage from "./components/WorldPage";

function App() {
	const { account, connect } = useWallet();

	return (
		<ChakraProvider>
			<Router>
				<Flex direction="column" align="center" pt={10} px={4}>
					<Heading as="h1" size="xl" mb={6}>
						🐉 Hydra Game Portal
					</Heading>

					<Routes>
						<Route
							path="/"
							element={
								account ? <Navigate to="/home" /> : <Navigate to="/register" />
							}
						/>
						<Route
							path="/register"
							element={<Register connect={connect} account={account} />}
						/>
						<Route path="/home" element={<Home account={account} />} />
						<Route path="/world/:id" element={<WorldPage />} />{" "}
					</Routes>
				</Flex>
			</Router>
		</ChakraProvider>
	);
}

export default App;
