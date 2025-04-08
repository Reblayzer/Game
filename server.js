const express = require("express");
const { spawn } = require("node:child_process");
const cors = require("cors");
const path = require("node:path");

const app = express();
app.use(cors());
app.use(express.json());

// Serve the built frontend files
app.use(express.static(path.join(__dirname, "frontend/build")));

app.post("/buy-plot", (req, res) => {
  const { worldId, player, x, y } = req.body;

  if (!worldId || !player || x === undefined || y === undefined) {
    return res.status(400).json({ error: "Missing required fields" });
  }

  const cmd = "npx";
  const args = [
    "tsx",
    "scripts/buyPlot-cli.ts",
    worldId,
    player,
    x.toString(),
    y.toString(),
  ];

  const child = spawn(cmd, args, { shell: true });

  let output = "";
  let error = "";

  child.stdout.on("data", (data) => {
    output += data.toString();
  });

  child.stderr.on("data", (data) => {
    error += data.toString();
  });

  child.on("close", (code) => {
    if (code === 0) {
      res.json({ message: output.trim() });
    } else {
      res.status(500).json({ error: error.trim() || "Unknown error" });
    }
  });
});

// Fallback to index.html for SPA routes
app.get("/*", (req, res) => {
  res.sendFile(path.join(__dirname, "frontend/build/index.html"));
});

const PORT = 3001;
app.listen(PORT, () => {
  console.log(`🚀 Unified app and API running at http://localhost:${PORT}`);
});