import { expect, test } from "@playwright/test";

test("student directory filters and paginates fictional records", async ({ page }) => {
  await page.goto("/admin/students");
  await expect(page.getByRole("heading", { name: "Student directory" })).toBeVisible();
  await page.getByRole("button", { name: "Next" }).click();
  await expect(page.getByText("Page 2 / 3")).toBeVisible();
  await page.getByLabel("Department", { exact: true }).fill("Computer Science");
  await page.getByLabel("Academic year", { exact: true }).fill("2025–2026");
  await page.getByRole("button", { name: "Search records", exact: true }).click();
  await expect(page.getByRole("button", { name: "Alex Morgan" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Maya Chen" })).toHaveCount(0);
  await page.getByRole("button", { name: "Alex Morgan" }).click();
  await expect(page.getByRole("heading", { name: "Team memberships" })).toBeVisible();
  await page.getByRole("button", { name: "Deactivate account" }).click();
  await expect(page.getByRole("dialog")).toContainText("Demonstration data");
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(
    page.getByRole("row", { name: /Alex Morgan/ }).getByRole("cell", { name: "Inactive", exact: true }),
  ).toBeVisible();
});

test("supervisor capacity uses the assigned project count and excludes the current supervisor", async ({
  page,
}) => {
  await page.goto("/admin/supervisors");
  await page.getByRole("button", { name: "Olivia Bennett" }).click();
  await expect(page.getByRole("heading", { name: "Campus Atlas" })).toBeVisible();
  await page.getByRole("button", { name: "Maximum projects", exact: true }).click();
  await expect(page.getByRole("spinbutton", { name: "Maximum projects" })).toHaveAttribute(
    "min",
    "1",
  );
  await page.getByRole("spinbutton", { name: "Maximum projects" }).fill("4");
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByRole("cell", { name: "1 / 4" })).toBeVisible();
  await page.getByRole("button", { name: "Olivia Bennett" }).click();
  await page.getByRole("button", { name: "Reassign supervisor" }).click();
  await expect(page.getByLabel("Available supervisor")).toBeVisible();
  await expect(page.getByRole("option", { name: /Olivia Bennett/ })).toHaveCount(0);
});

test("approved fictional proposal enters the assignment queue and updates workload", async ({
  page,
}) => {
  await page.goto("/admin/proposals");
  await page.getByRole("button", { name: "Library Lens" }).click();
  await expect(page.getByRole("heading", { name: "Team members" })).toBeVisible();
  await page.getByRole("button", { name: "Approve proposal" }).click();
  await page.getByRole("button", { name: "Confirm change" }).click();
  await page.getByRole("link", { name: "Supervisor assignments" }).click();
  await page.getByRole("button", { name: "Library Lens" }).click();
  await page.getByRole("button", { name: "Assign supervisor" }).click();
  await page.getByLabel("Available supervisor").selectOption("2");
  await expect(page.getByRole("dialog")).toContainText("0 / 4");
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByRole("button", { name: "Library Lens" })).toHaveCount(0);
  await page.getByRole("link", { name: "Supervisors", exact: true }).click();
  await page.getByRole("button", { name: "Daniel Reed" }).click();
  await expect(page.getByRole("heading", { name: "Library Lens" })).toBeVisible();
});

for (const width of [390, 768, 1440]) {
  test(`four module directories fit the ${width}px viewport`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    for (const domain of ["students", "supervisors", "proposals", "assignments"]) {
      await page.goto(`/admin/${domain}`);
      await expect(page.getByRole("main").getByRole("table")).toBeVisible();
      expect(
        await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth),
      ).toBe(true);
      if (process.env.CAPNEXA_SCREENSHOTS === "1") {
        await page.screenshot({
          path: test.info().outputPath(`${domain}-${width}.png`),
          fullPage: true,
        });
      }
    }
  });
}
