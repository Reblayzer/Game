const { ethers } = require("hardhat");
const path = require("node:path");
const fs = require("node:fs");

async function main() {
	const outputPath = path.join(
		__dirname,
		"../frontend/src/constants/addresses.ts",
	);

	if (!fs.existsSync(outputPath)) {
		throw new Error(`❌ Cannot find addresses.ts at: ${outputPath}`);
	}

	const raw = fs.readFileSync(outputPath, "utf8");
	const match = raw.match(/export const CONTRACTS = (\{[\s\S]*?\});/);

	if (!match) {
		throw new Error("❌ Failed to parse CONTRACTS object from addresses.ts");
	}

	let addresses;
	try {
		addresses = JSON.parse(match[1]);
	} catch (e) {
		throw new Error("❌ Failed to JSON.parse CONTRACTS from addresses.ts");
	}

	const playerNFTAddress = addresses.playerNFTAddress;
	const gameEngineAddress = addresses.gameEngineAddress;

	if (!playerNFTAddress || !gameEngineAddress) {
		throw new Error(
			"❌ Missing playerNFTAddress or gameEngineAddress in addresses.ts",
		);
	}

	console.log(
		`🔐 Whitelisting GameEngine (${gameEngineAddress}) in PlayerNFT (${playerNFTAddress})...`,
	);

	const PlayerNFT = await ethers.getContractAt("PlayerNFT", playerNFTAddress);
	const tx = await PlayerNFT.setWhitelistedCaller(gameEngineAddress, true);
	await tx.wait();

	console.log("✅ GameEngine successfully whitelisted in PlayerNFT.");
}

main().catch((error) => {
	console.error("❌ Whitelisting failed:", error);
	process.exitCode = 1;
});
