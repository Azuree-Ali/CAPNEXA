import { defineConfig } from "@playwright/test";

const previewUrl = "http://127.0.0.1:4271";
const contractUrl = "http://127.0.0.1:4272";
export default defineConfig({
  testDir: "./tests",
  workers: 1,
  use: { viewport: { width: 1440, height: 900 } },
  projects: [
    { name: "preview", testIgnore: "**/*-contract.spec.ts", use: { baseURL: previewUrl } },
    {
      name: "contract-fixtures",
      testMatch: "**/*-contract.spec.ts",
      use: { baseURL: contractUrl },
    },
  ],
  webServer: [
    {
      command: "npm run dev -- --host 127.0.0.1 --port 4271 --strictPort",
      url: previewUrl,
      env: { VITE_AUTH_MODE: "preview" },
      reuseExistingServer: false,
    },
    {
      command: "npm run dev -- --host 127.0.0.1 --port 4272 --strictPort",
      url: contractUrl,
      env: { VITE_AUTH_MODE: "live", VITE_API_BASE_URL: "https://capnexa.test" },
      reuseExistingServer: false,
    },
  ],
});
