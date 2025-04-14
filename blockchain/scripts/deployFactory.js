const { ethers } = require("hardhat");
const fs = require("node:fs");
const path = require("node:path");

async function main() {
	const addressesPath = path.join(
		__dirname,
		"../frontend/src/constants/addresses.ts",
	);

	// 👇 Read & parse existing addresses.ts
	const raw = fs.readFileSync(addressesPath, "utf8");
	const match = raw.match(/export const CONTRACTS = (\{[\s\S]*?\});/);
	if (!match)
		throw new Error("Could not find CONTRACTS object in addresses.ts");

	const current = JSON.parse(match[1]);
	const playerNFTAddress = current.playerNFTAddress;

	if (!playerNFTAddress || !ethers.utils.isAddress(playerNFTAddress)) {
		throw new Error("❌ playerNFTAddress missing or invalid in addresses.ts");
	}

	// ✅ Deploy factory
	const Factory = await ethers.getContractFactory("GameWorldFactory");
	const factory = await Factory.deploy(playerNFTAddress);
	await factory.deployed();

	console.log("✅ GameWorldFactory deployed at:", factory.address);

	// 📝 Update and save addresses.ts
	current.factoryAddress = factory.address;

	const output = `// Auto-generated\nexport const CONTRACTS = ${JSON.stringify(current, null, 2)};\n`;
	fs.writeFileSync(addressesPath, output);
	console.log("📁 addresses.ts updated ✅");
}

main().catch((error) => {
	console.error("❌ Factory deployment failed:", error);
	process.exit(1);
});
