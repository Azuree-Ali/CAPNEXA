import { expect, test } from "@playwright/test";

for (const width of [390, 768, 1440]) {
  test(`selectable wordmark and footer at ${width}px`, async ({ page }) => {
    await page.setViewportSize({ width, height: 900 });
    for (const path of ["/login", "/forgot-password", "/admin/dashboard", "/supervisor/profile"]) {
      await page.goto(path);
      const footer = page.getByRole("contentinfo");
      const mark = footer.locator(".capnexa-brand__wordmark");
      await expect(mark).toHaveText("CAPNEXA");
      const selected = await mark.evaluate(async (element) => {
        await document.fonts.ready;
        const range = document.createRange();
        range.selectNodeContents(element);
        const selection = window.getSelection();
        selection?.removeAllRanges();
        selection?.addRange(range);
        return {
          text: selection?.toString(),
          font: document.fonts.check('16px "Capnexa Wordmark"'),
          fits: document.documentElement.scrollWidth <= innerWidth,
        };
      });
      expect(selected).toEqual({ text: "CAPNEXA", font: true, fits: true });
      for (const name of ["Privacy", "Help"]) {
        const button = footer.getByRole("button", { name, exact: true });
        const box = await button.boundingBox();
        expect(box?.width).toBeGreaterThanOrEqual(44);
        expect(box?.height).toBeGreaterThanOrEqual(44);
        await button.click();
        await expect(page.locator("dialog[open]")).toBeVisible();
        await page.keyboard.press("Escape");
        await expect(button).toBeFocused();
      }
    }
  });
}
test("canonical student edit returns to the same profile", async ({ page }) => {
  await page.goto("/admin/students?preview-tools");
  await page.getByRole("link", { name: "Alex Morgan" }).click();
  for (const name of ["Save changes", "Cancel", "Close"]) {
    await page.getByRole("button", { name: "Edit student details" }).click();
    await page.getByRole("dialog").getByRole("button", { name, exact: true }).click();
    await expect(page.getByRole("dialog")).toHaveCount(0);
    await expect(page).toHaveURL(/\/admin\/students\/alex-morgan$/);
  }
});

test("recovery is explicitly simulated and password visibility has a 44px target", async ({
  page,
}) => {
  for (const width of [390, 768, 1440]) {
    await page.setViewportSize({ width, height: 900 });
    await page.goto("/login");
    const toggle = page.getByRole("button", { name: "Show password" });
    const box = await toggle.boundingBox();
    expect(box?.width).toBeGreaterThanOrEqual(44);
    expect(box?.height).toBeGreaterThanOrEqual(44);
  }
  await page.goto("/forgot-password");
  await expect(
    page.getByText("This is a demonstration. No recovery email will be sent."),
  ).toBeVisible();
  await page.locator('input[name="email"]').fill("student@example.edu");
  await page.getByRole("button", { name: "Simulate Recovery Request" }).click();
  await expect(page.getByText("Preview only. No recovery email was sent.")).toBeVisible();
});
