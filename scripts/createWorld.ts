import type { ethers as ethersLib } from "ethers";
import { ethers } from "hardhat";
import fs from "node:fs";
import path from "node:path";

async function main() {
  const addressesPath = path.join(__dirname, "../frontend/src/constants/addresses.ts");
  const raw = fs.readFileSync(addressesPath, "utf8");
  const match = raw.match(/export const CONTRACTS = (\{[\s\S]*?\});/);

  if (!match) throw new Error("Could not find CONTRACTS object in addresses.ts");

  const current = JSON.parse(match[1]);
  const factoryAddress = current.factoryAddress;

  const factory = await ethers.getContractAt("GameWorldFactory", factoryAddress);

  console.log("🛠 Creating new game world...");
  const tx = await factory.createWorld("World X", 1, 12345); // name, difficulty, seed
  const receipt = await tx.wait();

  const event = receipt.logs
    .map((log: ethersLib.providers.Log) => {
      try {
        return factory.interface.parseLog(log);
      } catch {
        return null;
      }
    })
    .find((log: ethersLib.providers.Log) => {
      const parsedLog = factory.interface.parseLog(log);
      return parsedLog?.name === "WorldCreated";
    });

  if (!event) throw new Error("❌ No WorldCreated event found");

  const worldAddress = event.args.world;
  console.log("✅ New GameEngine deployed at:", worldAddress);

  // 📝 Add to addresses.ts (optional - stores latest)
  current.lastWorldAddress = worldAddress;
  const output = `// Auto-generated\nexport const CONTRACTS = ${JSON.stringify(current, null, 2)};\n`;
  fs.writeFileSync(addressesPath, output);
  console.log("📁 addresses.ts updated ✅");
}

main().catch((err) => {
  console.error("❌ createWorld failed:", err);
  process.exit(1);
});
