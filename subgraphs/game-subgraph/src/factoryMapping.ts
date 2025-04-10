import { WorldCreated } from "../generated/GameWorldFactory/GameWorldFactory";
import { GameWorld } from "../generated/schema";
import { GameEngineTemplate } from "../generated/templates";
import { Address } from "@graphprotocol/graph-ts";

export function handleWorldCreated(event: WorldCreated): void {
  const worldAddress = event.params.gameEngine;
  const id = worldAddress.toHex();

  const world = new GameWorld(id);
  world.name = event.params.name;
  world.createdAt = event.block.timestamp;

  world.save();

  GameEngineTemplate.create(worldAddress);
}
