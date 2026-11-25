import { expect, test } from "@playwright/test";
import { readFile } from "node:fs/promises";

test("configuration validates dates, retains preview edits and exposes the roles matrix", async ({
  page,
}) => {
  await page.goto("/admin/settings");
  await page.getByRole("button", { name: "Add record" }).click();
  const dialog = page.getByRole("dialog");
  await dialog.getByLabel("Academic year").fill("2026–2027");
  await dialog.getByLabel("Start date").fill("2026-09-01");
  await dialog.getByLabel("End date").fill("2026-06-01");
  await dialog.getByRole("button", { name: "Save preview changes" }).click();
  await expect(dialog.getByRole("alert")).toBeVisible();
  await dialog.getByLabel("End date").fill("2027-06-01");
  await dialog.getByRole("button", { name: "Save preview changes" }).click();
  await expect(page.getByRole("rowheader", { name: "2026–2027" })).toBeVisible();
  await page.getByRole("link", { name: "Departments", exact: true }).click();
  await page.getByRole("link", { name: "Academic years", exact: true }).click();
  await expect(page.getByRole("rowheader", { name: "2026–2027" })).toBeVisible();
  await page.getByRole("link", { name: "Roles and permissions" }).click();
  await expect(page.getByRole("columnheader", { name: "Student" })).toBeVisible();
  await page.getByLabel("Capability area").selectOption("Delivery");
  await expect(page.getByRole("rowheader", { name: /Submit deliverables/ })).toBeVisible();
  await expect(page.getByRole("rowheader", { name: /Review proposals/ })).toHaveCount(0);
});

test("report exports exactly the filtered preview and handles empty filters", async ({ page }) => {
  await page.goto("/admin/reports");
  await page.getByLabel("Report template").selectOption("projects");
  await page.getByLabel("Department").selectOption("Information Systems");
  await expect(page.getByRole("rowheader", { name: "Library Lens" })).toBeVisible();
  await expect(page.getByRole("rowheader", { name: "Campus Atlas" })).toHaveCount(0);
  const downloading = page.waitForEvent("download");
  await page.getByRole("button", { name: "Download CSV" }).click();
  const downloaded = await downloading;
  const csv = await readFile(await downloaded.path(), "utf8");
  expect(csv).toContain("Library Lens");
  expect(csv).not.toContain("Campus Atlas");
  await page.getByLabel("Report template").selectOption("supervisors");
  await expect(page.getByText("No matching records.")).toBeVisible();
  await expect(page.getByRole("button", { name: "Download CSV" })).toBeDisabled();
});

test("milestone templates preserve editable fields and keyboard dismissal", async ({ page }) => {
  await page.goto("/admin/milestones");
  const add = page.getByRole("button", { name: "Add milestone" });
  await add.focus();
  await page.keyboard.press("Enter");
  const dialog = page.getByRole("dialog");
  await dialog.getByLabel("Milestone template").selectOption("Final defense");
  await expect(dialog.getByLabel("Milestone", { exact: true })).toHaveValue("Final defense");
  await page.keyboard.press("Escape");
  await expect(add).toBeFocused();
  await add.click();
  await dialog.getByLabel("Milestone template").selectOption("Final defense");
  await dialog.getByLabel("Due date").fill("2026-06-15");
  await dialog.getByRole("button", { name: "Save preview changes" }).click();
  await expect(page.getByRole("rowheader", { name: "Final defense" })).toBeVisible();
});

test("advanced previews support ambiguous searches and reversible review", async ({ page }) => {
  await page.goto("/admin/assistant");
  await page.getByLabel("Academic question").fill("design specification and design brief");
  await page.getByRole("button", { name: "Find answer" }).click();
  await expect(page.getByText("More than one source matches.", { exact: false })).toBeVisible();
  await page.goto("/admin/monitoring");
  await page.getByLabel("Signal category").selectOption("Academic delivery");
  await page.getByText("Late design revision").click();
  await page.getByRole("button", { name: "Mark reviewed" }).click();
  await expect(page.getByRole("button", { name: "Reopen review" })).toBeVisible();
  await page.getByRole("button", { name: "Reopen review" }).click();
  await expect(page.getByRole("button", { name: "Mark reviewed" })).toBeVisible();
});

test("modified workspaces stay within the viewport at all supported widths", async ({ page }) => {
  const paths = [
    "/admin/settings",
    "/admin/settings/permissions",
    "/admin/reports",
    "/admin/milestones",
    "/admin/assistant",
    "/admin/monitoring",
    "/admin/analytics",
    "/forbidden",
    "/session-expired",
    "/missing-page",
  ];
  const runtimeErrors: string[] = [];
  page.on("pageerror", (error) => runtimeErrors.push(error.message));
  for (const width of [390, 768, 1440]) {
    await page.setViewportSize({ width, height: 900 });
    for (const path of paths) {
      await page.goto(path);
      await expect(page.locator("main h1"), path).toBeVisible();
      expect(await page.evaluate(() => document.documentElement.scrollWidth), path).toBe(width);
    }
  }
  expect(runtimeErrors).toEqual([]);
});
