import { expect, test } from "@playwright/test";
import type { AuthResponseDto } from "../src/features/auth/types/identity.types";
import type { AdminSupervisorListItemDto } from "../src/features/supervisors/types/supervisors.types";
import type { AdminUnassignedProjectListItemDto } from "../src/features/assignments/types/assignments.types";

const account: AuthResponseDto = {
  accessToken: "fixture-admin",
  tokenType: "Bearer",
  expiresInSeconds: 1800,
  refreshToken: "fixture",
  user: {
    id: "admin",
    email: "admin@example.edu",
    firstName: "Test",
    lastName: "Admin",
    roles: ["Admin"],
  },
};
const candidate: AdminSupervisorListItemDto = {
  supervisorId: 4,
  userId: "staff-4",
  firstName: "Olivia",
  lastName: "Bennett",
  email: null,
  isActive: true,
  department: null,
  specialization: null,
  researchInterests: null,
  maxProjects: 3,
  assignedProjectsCount: 1,
  availableCapacity: 2,
};
const project: AdminUnassignedProjectListItemDto = {
  projectId: 7,
  title: "Fixture campus project",
  reviewedAt: null,
  team: { teamId: 2, name: "Atlas", teamCode: "ATL", memberCount: 3 },
  leaderName: "Test Student",
};
function paged<T>(items: T[]) {
  return {
    items,
    page: 1,
    pageSize: 20,
    totalCount: items.length,
    totalPages: items.length ? 1 : 0,
  };
}
test("assigns a verified candidate only after a confirmed server response", async ({ page }) => {
  let candidatesAvailable = false;
  let assigned = false;
  await page.route("https://capnexa.test/api/**", async (route) => {
    const url = new URL(route.request().url());
    if (url.pathname === "/api/Identity/Auth/login") return route.fulfill({ json: account });
    expect(route.request().headers().authorization).toBe("Bearer fixture-admin");
    switch (url.pathname) {
      case "/api/Admin/Students":
        return route.fulfill({ json: paged([]) });
      case "/api/Admin/ProjectAssignments/unassigned":
        return route.fulfill({ json: paged(assigned ? [] : [project]) });
      case "/api/Admin/Supervisors":
        expect(url.searchParams.get("isActive")).toBe("true");
        expect(url.searchParams.get("hasCapacity")).toBe("true");
        if (!candidatesAvailable)
          return route.fulfill({ status: 503, json: { message: "Fixture unavailable" } });
        return route.fulfill({ json: paged([candidate]) });
      case "/api/Admin/ProjectAssignments/7":
        expect(route.request().method()).toBe("PUT");
        expect(route.request().postDataJSON()).toEqual({ supervisorId: 4 });
        assigned = true;
        return route.fulfill({ json: { ...project, status: "Approved", supervisor: candidate } });
      default:
        throw new Error("Unexpected fixture route: " + url.pathname);
    }
  });
  await page.goto("/admin/assignments");
  await expect(page).toHaveURL(/\/login$/);
  await page.locator('input[name="email"]').fill("admin@example.edu");
  await page.locator('input[name="password"]').fill("fixture-password");
  await page.getByRole("button", { name: "Sign In", exact: true }).click();
  await page.getByRole("link", { name: "Assignments", exact: true }).click();
  await page.getByRole("button", { name: project.title }).click();
  await page.getByRole("button", { name: "Assign supervisor", exact: true }).click();
  const dialog = page.getByRole("dialog");
  const confirm = dialog.getByRole("button", { name: "Confirm change" });
  await expect(confirm).toBeDisabled();
  await expect(dialog.getByRole("alert")).toContainText("Unable to load available supervisors");
  candidatesAvailable = true;
  await dialog.getByRole("button", { name: "Retry" }).click();
  await dialog.getByLabel("Available supervisor").selectOption("4");
  await confirm.click();
  await expect(page.getByText("No matching records.")).toBeVisible();
  expect(assigned).toBe(true);
});

test("proposals expose team and review details while connected milestones stay read-only", async ({
  page,
}) => {
  const proposalId = 12;
  const team = {
    teamId: 2,
    name: "Atlas",
    teamCode: "ATL",
    maxMembers: 4,
    memberCount: 1,
    members: [
      {
        userId: "fixture-student",
        firstName: "Alex",
        lastName: "Morgan",
        email: null,
        studentNumber: "ST-001",
        department: null,
        academicYear: null,
        isLeader: true,
        joinedAt: "2026-09-01T10:00:00Z",
      },
    ],
  };
  const summary = {
    projectId: proposalId,
    title: "Proposal fixture",
    submittedAt: "2026-10-01T10:00:00Z",
    reviewedAt: null,
    team,
    leaderName: "Alex Morgan",
  };
  let decision = "PendingReview";
  const detail = () => ({
    ...summary,
    status: decision,
    description: "The fictional proposal scope.",
    createdAt: summary.submittedAt,
    review:
      decision === "Rejected"
        ? {
            decision,
            reason: "Clarify scope",
            reviewedByUserId: "admin",
            reviewedByName: "Test Admin",
            reviewedAt: "2026-10-02T10:00:00Z",
          }
        : null,
  });
  await page.route("https://capnexa.test/api/**", async (route) => {
    const url = new URL(route.request().url());
    if (url.pathname === "/api/Identity/Auth/login") return route.fulfill({ json: account });
    expect(route.request().headers().authorization).toBe("Bearer fixture-admin");
    switch (url.pathname) {
      case "/api/Admin/Students":
        return route.fulfill({ json: paged([]) });
      case "/api/Admin/ProjectProposals": {
        const status = url.searchParams.get("status");
        expect(["PendingReview", "Rejected"]).toContain(status);
        return route.fulfill({
          json: paged(status === decision ? [{ ...summary, status: decision }] : []),
        });
      }
      case "/api/Admin/ProjectProposals/12":
        return route.fulfill({ json: detail() });
      case "/api/Admin/ProjectProposals/12/review":
        expect(route.request().method()).toBe("PUT");
        expect(route.request().postDataJSON()).toEqual({
          decision: "Rejected",
          reason: "Clarify scope",
        });
        decision = "Rejected";
        return route.fulfill({ json: detail() });
      default:
        throw new Error("Unexpected fixture route: " + url.pathname);
    }
  });
  await page.goto("/login");
  await page.locator('input[name="email"]').fill("admin@example.edu");
  await page.locator('input[name="password"]').fill("fixture-password");
  await page.getByRole("button", { name: "Sign In", exact: true }).click();
  await page.getByRole("link", { name: "Proposals", exact: true }).click();
  await page.getByRole("button", { name: summary.title }).click();
  await expect(page.getByText("Alex Morgan (Team leader)")).toBeVisible();
  await page.getByRole("button", { name: "Reject proposal" }).click();
  const dialog = page.getByRole("dialog");
  await dialog.getByLabel("Decision reason").fill("Clarify scope");
  await dialog.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByText("No matching records.")).toBeVisible();
  await page.getByLabel("Status", { exact: true }).selectOption("Rejected");
  await page.getByRole("button", { name: summary.title }).click();
  await expect(page.getByText("Clarify scope")).toBeVisible();
  await expect(page.locator("main").getByText("Test Admin", { exact: true })).toBeVisible();
  await expect(page.getByRole("button", { name: "Reject proposal" })).toHaveCount(0);
  await page.getByRole("link", { name: "Milestones", exact: true }).click();
  await expect(
    page.getByText("Milestone editing awaits role ownership", { exact: false }),
  ).toBeVisible();
  await expect(page.getByRole("button", { name: "Add milestone" })).toHaveCount(0);
});

test("supervisors filter by status/capacity and reassign approved projects with responsive views", async ({
  page,
}) => {
  let reassigned = false;
  const replacement: AdminSupervisorListItemDto = {
    ...candidate,
    supervisorId: 5,
    userId: "staff-5",
    firstName: "Daniel",
    lastName: "Reed",
  };
  await page.route("https://capnexa.test/api/**", async (route) => {
    const url = new URL(route.request().url());
    if (url.pathname === "/api/Identity/Auth/login") return route.fulfill({ json: account });
    expect(route.request().headers().authorization).toBe("Bearer fixture-admin");
    switch (url.pathname) {
      case "/api/Admin/Students":
        return route.fulfill({ json: paged([]) });
      case "/api/Admin/Supervisors": {
        const isActive = url.searchParams.get("isActive");
        const hasCapacity = url.searchParams.get("hasCapacity");
        if (isActive === "false") return route.fulfill({ json: paged([]) });
        if (hasCapacity === "true") return route.fulfill({ json: paged([candidate, replacement]) });
        return route.fulfill({
          json: paged([
            {
              ...candidate,
              assignedProjectsCount: reassigned ? 0 : 1,
              availableCapacity: reassigned ? 3 : 2,
            },
          ]),
        });
      }
      case "/api/Admin/Supervisors/4":
        return route.fulfill({
          json: {
            ...candidate,
            assignedProjectsCount: reassigned ? 0 : 1,
            availableCapacity: reassigned ? 3 : 2,
            assignedProjects: reassigned
              ? []
              : [
                  {
                    projectId: 7,
                    title: project.title,
                    status: "Approved",
                    teamId: 2,
                    teamName: "Atlas",
                    teamCode: "ATL",
                  },
                ],
          },
        });
      case "/api/Admin/ProjectAssignments/7":
        expect(route.request().method()).toBe("PUT");
        expect(route.request().postDataJSON()).toEqual({ supervisorId: 5 });
        reassigned = true;
        return route.fulfill({ json: { ...project, status: "Approved", supervisor: replacement } });
      default:
        throw new Error("Unexpected fixture route: " + url.pathname);
    }
  });

  await page.goto("/login");
  await page.locator('input[name="email"]').fill("admin@example.edu");
  await page.locator('input[name="password"]').fill("fixture-password");
  await page.getByRole("button", { name: "Sign In", exact: true }).click();
  await page.getByRole("link", { name: "Supervisors", exact: true }).click();
  await expect(page.getByRole("heading", { name: "Supervisor directory" })).toBeVisible();
  await page.getByLabel("Account status").selectOption("false");
  await expect(page.getByText("No matching records.")).toBeVisible();
  await page.getByRole("button", { name: "Clear filters" }).click();
  await page.getByRole("button", { name: "Olivia Bennett", exact: true }).click();
  await expect(page.getByRole("heading", { name: project.title })).toBeVisible();
  await page.getByRole("button", { name: "Reassign supervisor" }).click();
  const dialog = page.getByRole("dialog");
  await expect(dialog.getByRole("option", { name: /Olivia Bennett/ })).toHaveCount(0);
  await dialog.getByLabel("Available supervisor").selectOption("5");
  await dialog.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByText("Supervisor reassigned.")).toBeVisible();
  await expect(page.getByText("No assigned projects are recorded.")).toBeVisible();
  expect(reassigned).toBe(true);

  for (const width of [390, 768, 1440] as const) {
    await page.setViewportSize({ width, height: 900 });
    const overflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth,
    );
    expect(overflow).toBe(false);
  }
});
