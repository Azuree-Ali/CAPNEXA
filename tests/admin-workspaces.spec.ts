import { expect, test } from "@playwright/test";

test("project views retain their URL and open the delivery timeline", async ({ page }) => {
  await page.goto("/admin/projects");
  await page.getByLabel("Project stage").selectOption("Design");
  await expect(page.getByText("Library Lens")).toHaveCount(0);
  await page.getByRole("link", { name: "Campus Atlas" }).click();
  await page
    .getByRole("navigation", { name: "Project workspace views" })
    .getByRole("link", { name: "Milestones" })
    .click();
  await expect(page).toHaveURL(/\?view=milestones$/);
  await page.reload();
  await expect(page.getByRole("heading", { name: "Upcoming deliverables" })).toBeVisible();
  await page.getByRole("link", { name: "Open project timeline" }).click();
  await expect(page.getByRole("heading", { name: "Design specification" })).toBeVisible();
});

test("submission inspector opens the corresponding review history", async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 900 });
  await page.goto("/admin/submissions");
  await page.getByLabel("Review status").selectOption("Awaiting review");
  await expect(page.getByText("Atlas route mapping outline")).toHaveCount(0);
  await page.getByText("Atlas design brief", { exact: true }).click();
  await page.getByRole("link", { name: "Review history" }).click();
  await expect(page.getByRole("heading", { name: "v1 · Initial design brief" })).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBe(390);
});

test("assistant links an answer to evidence and handles missing evidence", async ({ page }) => {
  await page.goto("/admin/assistant");
  await page.getByRole("button", { name: "When is the design specification due?" }).click();
  await expect(page.getByRole("link", { name: "Atlas milestone schedule" })).toBeVisible();
  await page.getByLabel("Academic question").fill("What is the defense venue?");
  await page.getByRole("button", { name: "Find answer" }).click();
  await expect(page.getByText("No verified answer is available for this query.")).toBeVisible();
  await expect(page.getByRole("link", { name: "Atlas milestone schedule" })).toHaveCount(0);
});

test("compares selected supervisors and filters unread notifications", async ({ page }) => {
  await page.goto("/admin/recommendations");
  await page.getByRole("checkbox", { name: /Olivia Bennett/ }).check();
  await page.getByRole("checkbox", { name: /Sam Reed/ }).check();
  await page.getByRole("link", { name: "Compare selected candidates" }).click();
  await expect(page.getByRole("columnheader", { name: "Dr. Sam Reed" })).toBeVisible();
  await page.goto("/admin/notifications?view=unread");
  const mark = page.getByRole("button", { name: "Mark as read" });
  await mark.first().click();
  await mark.first().click();
  await expect(page.getByText("You have no unread updates.")).toBeVisible();
});
