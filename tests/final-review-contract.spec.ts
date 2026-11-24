import { expect, test, type Page } from "@playwright/test";
import type { AuthResponseDto } from "../src/features/auth/types/identity.types";

const apiOrigin = "https://capnexa.test";
const loginEndpoint = apiOrigin + "/api/Identity/Auth/login";
const admin: AuthResponseDto = {
  accessToken: "fixture",
  tokenType: "Bearer",
  expiresInSeconds: 120,
  refreshToken: "fixture",
  user: {
    id: "fixture-admin",
    email: "admin@example.edu",
    firstName: "Fixture",
    lastName: "Admin",
    roles: ["Admin"],
  },
};

async function submitLogin(page: Page) {
  await page.locator('input[name="email"]').fill("admin@example.edu");
  await page.locator('input[name="password"]').fill("fixture-password");
  await page.getByRole("button", { name: "Sign In", exact: true }).click();
}

for (const outcome of [200, 401] as const) {
  test(`ignores a late login response with status ${outcome} after leaving sign-in`, async ({
    page,
  }) => {
    let release: () => void = () => {};
    const gate = new Promise<void>((resolve) => {
      release = resolve;
    });
    await page.route(loginEndpoint, async (route) => {
      await gate;
      await route
        .fulfill({ status: outcome, json: outcome === 200 ? admin : { message: "Denied" } })
        .catch(() => undefined);
    });
    await page.goto("/login");
    const started = page.waitForRequest(loginEndpoint);
    await submitLogin(page);
    const request = await started;
    const settled = Promise.race([
      page.waitForEvent("requestfailed", { predicate: (failed) => failed === request }),
      page.waitForEvent("requestfinished", { predicate: (finished) => finished === request }),
    ]);
    await page.getByRole("link", { name: "Forgot password?" }).click();
    release();
    await settled;
    await expect(page).toHaveURL(/\/forgot-password$/);
    await expect(
      page.getByText("Online password recovery is unavailable.", { exact: false }),
    ).toBeVisible();
    await expect(page.locator('input[name="email"]')).toHaveCount(0);
    // Navigate inside the SPA to exercise the original tab's memory session.
    await page.getByRole("link", { name: "Back to sign-in" }).click();
    await page.evaluate(() => {
      history.pushState(null, "", "/admin/dashboard");
      window.dispatchEvent(new PopStateEvent("popstate"));
    });
    await expect(page).toHaveURL(/\/login$/);
  });
}

test("connected Admin and Supervisor see only available profile destinations", async ({ page }) => {
  let role: "Admin" | "Supervisor" = "Admin";
  await page.route(loginEndpoint, (route) =>
    route.fulfill({ json: { ...admin, user: { ...admin.user, roles: [role] } } }),
  );
  await page.route(/^https:\/\/capnexa\.test\/api\/Admin\/Students(?:\?|$)/, (route) =>
    route.fulfill({
      status: 400,
      json: { title: "Validation failed", errors: { page: ["Invalid page value."] } },
    }),
  );
  await page.goto("/login");
  await submitLogin(page);
  await expect(page.getByRole("alert")).toContainText("Invalid page value.");
  const search = page.getByRole("combobox", { name: "Search workspace" });
  await search.fill("Office");
  await expect(page.getByRole("option", { name: "Office & Availability" })).toHaveCount(0);
  await search.fill("Atlas team");
  await page.getByRole("option", { name: "Atlas team" }).click();
  await expect(page.getByText("Alex Morgan")).toBeVisible();
  await expect(page.getByRole("link", { name: "Alex Morgan" })).toHaveCount(0);
  await expect(page.getByRole("button", { name: "Open preferences" })).toHaveCount(0);

  role = "Supervisor";
  await page.goto("/login");
  await submitLogin(page);
  await expect(page).toHaveURL(/\/supervisor\/profile$/);
  await expect(page.getByRole("button", { name: "Open preferences" })).toHaveCount(0);
  await search.fill("account profile");
  await page.getByRole("option", { name: "Account profile" }).click();
  await expect(page).toHaveURL(/\/supervisor\/profile$/);
});
