import { PlayerNFT } from "../generated/schema";
import { BigInt } from "@graphprotocol/graph-ts";

export function ensurePlayerNFT(playerId: string, timestamp: BigInt): PlayerNFT {
  let playerNFT = PlayerNFT.load(playerId);
  if (!playerNFT) {
    playerNFT = new PlayerNFT(playerId);
    playerNFT.tokenId = BigInt.zero();
    playerNFT.username = "unknown";
    playerNFT.registeredAt = timestamp;
    playerNFT.save();
  }
  return playerNFT;
}