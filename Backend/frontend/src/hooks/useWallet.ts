import { useState, useEffect } from "react";
import { BrowserProvider } from "ethers";

export function useWallet() {
  const [account, setAccount] = useState<string | null>(null);
  const [provider, setProvider] = useState<BrowserProvider | null>(null);

  const connect = async () => {
    if ((window as any).ethereum) {
      const ethProvider = new BrowserProvider((window as any).ethereum);
      const accounts = await (window as any).ethereum.request({ method: "eth_requestAccounts" });

      setAccount(accounts[0]);
      setProvider(ethProvider);
    }
  };

  useEffect(() => {
    connect();
  }, []);

  return { account, provider, connect };
}