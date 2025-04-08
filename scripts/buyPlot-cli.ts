// scripts/buyPlot-cli.ts
import { ethers } from "ethers";
import * as dotenv from "dotenv";
import { CONTRACTS } from "../frontend/src/constants/addresses";
import GameEngineABI from "../artifacts/contracts/GameEngine.sol/GameEngine.json";

dotenv.config();

async function main() {
  const args = process.argv.slice(2);
  const [worldId, player, x, y] = args;

  if (!worldId || !player || x === undefined || y === undefined) {
    console.error("❌ Usage: npx tsx scripts/buyPlot-cli.ts <worldId> <player> <x> <y>");
    process.exit(1);
  }

  const provider = new ethers.providers.JsonRpcProvider("https://rpc-testnet.hydrachain.org");
  if (!process.env.PRIVATE_KEY) {
    console.error("❌ PRIVATE_KEY is not defined in the environment variables.");
    process.exit(1);
  }
  const wallet = new ethers.Wallet(process.env.PRIVATE_KEY, provider);
  const gameEngine = new ethers.Contract(CONTRACTS.gameEngineAddress, GameEngineABI.abi, wallet);

  console.log(`📦 Buying plot (${x}, ${y}) in World ${worldId} for player ${player}...`);

  try {
    const tx = await gameEngine.buyPlot(worldId, player, x, y);
    await tx.wait();
    console.log("✅ Plot purchased successfully!");
  } catch (err) {
    console.error("❌ Error purchasing plot:", err);
  }
}

main();
