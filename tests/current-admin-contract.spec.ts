import { expect, test, type Page } from "@playwright/test";
import type {
  AdminStudentListItemDto,
  StudentProfileDto,
} from "../src/features/students/types/students.types";
import type {
  AdminSupervisorDetailDto,
  AdminSupervisorListItemDto,
} from "../src/features/supervisors/types/supervisors.types";
import type {
  AdminProposalDetailDto,
  AdminProposalListItemDto,
} from "../src/features/proposals/types/proposals.types";
import type { AuthResponseDto } from "../src/features/auth/types/identity.types";

const origin = "https://capnexa.test";
const auth: AuthResponseDto = {
  accessToken: "fixture-admin",
  tokenType: "Bearer",
  expiresInSeconds: 120,
  refreshToken: "fixture-refresh",
  user: {
    id: "reviewer",
    email: "admin@example.edu",
    firstName: "Fixture",
    lastName: "Admin",
    roles: ["Admin"],
  },
};
const date = "2025-12-10T12:00:00Z";
const sampleStudent: AdminStudentListItemDto = {
  userId: "student-1",
  email: "alex@example.edu",
  firstName: "Alex",
  lastName: "Morgan",
  studentNumber: "STU-021",
  department: "Computer Science",
  academicYear: "2025–2026",
  academicProfileComplete: true,
  emailConfirmed: true,
  isActive: true,
  createdAt: date,
};
const sampleSupervisor: AdminSupervisorListItemDto = {
  supervisorId: 4,
  userId: "faculty-4",
  firstName: "Olivia",
  lastName: "Bennett",
  email: "olivia@example.edu",
  isActive: true,
  department: "Computer Science",
  specialization: "Distributed systems",
  researchInterests: "Accessible campus platforms",
  maxProjects: 3,
  assignedProjectsCount: 1,
  availableCapacity: 2,
};
const team = { teamId: 31, name: "Lens", teamCode: "TEAM-31", memberCount: 1 };
const proposal: AdminProposalListItemDto = {
  projectId: 31,
  title: "Library Lens",
  status: "PendingReview",
  submittedAt: date,
  reviewedAt: null,
  team,
  leaderName: "Priya Shah",
};
function listing<T>(items: T[]) {
  return { items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1 };
}
async function login(page: Page, session = auth) {
  await page.route(origin + "/api/Identity/Auth/login", async (route) => {
    expect(route.request().method()).toBe("POST");
    expect(route.request().postDataJSON()).toEqual({
      email: "admin@example.edu",
      password: "fixture-password",
    });
    await route.fulfill({ json: session });
  });
  await page.goto("/login");
  await page.locator('input[name="email"]').fill("admin@example.edu");
  await page.locator('input[name="password"]').fill("fixture-password");
  await page.getByRole("button", { name: "Sign In", exact: true }).click();
}

test("student details retry, cancel, deactivate, and reactivate with refreshed records", async ({
  page,
}) => {
  let active = true;
  let failDetails = true;
  const writes: boolean[] = [];
  await page.route(/^https:\/\/capnexa\.test\/api\/Admin\/Students(?:[/?]|$)/, async (route) => {
    const request = route.request();
    expect(request.headers().authorization).toBe("Bearer fixture-admin");
    const path = new URL(request.url()).pathname;
    if (path.endsWith("/profile")) {
      if (failDetails) {
        await route.fulfill({
          status: 503,
          json: { message: "Student records are temporarily unavailable." },
        });
        return;
      }
      const profile: StudentProfileDto = {
        ...sampleStudent,
        isActive: active,
        level: "Fourth year",
        teams: [
          {
            teamId: 26,
            name: "Atlas",
            teamCode: "TEAM-26",
            isLeader: true,
            isCreator: true,
            joinedAt: date,
            projects: [{ projectId: 26, title: "Campus Atlas", status: "Active", createdAt: date }],
          },
        ],
      };
      await route.fulfill({ json: profile });
    } else if (path.endsWith("/status")) {
      expect(request.method()).toBe("PUT");
      expect(request.postDataJSON()).toEqual({ isActive: !active });
      active = !active;
      writes.push(active);
      await route.fulfill({ json: { userId: sampleStudent.userId, isActive: active } });
    } else {
      const url = new URL(request.url());
      const filter = url.searchParams.get("isActive");
      const matchesFilter = filter === null || filter === String(active);
      await route.fulfill({
        json: listing(matchesFilter ? [{ ...sampleStudent, isActive: active }] : []),
      });
    }
  });

  await login(page);
  const student = page.getByRole("button", { name: "Alex Morgan", exact: true });
  await student.click();
  await expect(page.getByRole("alert")).toContainText("temporarily unavailable");
  failDetails = false;
  await page.getByRole("button", { name: "Retry" }).click();
  await expect(page.getByRole("main").getByText("Campus Atlas", { exact: true })).toBeVisible();
  await page.getByRole("button", { name: "Deactivate account" }).click();
  await page.getByRole("dialog").getByRole("button", { name: "Cancel" }).click();
  await expect(page.getByRole("dialog")).toHaveCount(0);
  expect(writes).toEqual([]);
  await page.getByRole("button", { name: "Deactivate account" }).click();
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByText("Student account deactivated.")).toBeVisible();
  await expect(page.getByRole("cell", { name: "Inactive", exact: true })).toBeVisible();
  await page.getByLabel("Account status").selectOption("true");
  await expect(page.getByText("No matching records.")).toBeVisible();
  await page.getByRole("button", { name: "Clear filters" }).click();
  await student.click();
  await page.getByRole("button", { name: "Activate account" }).click();
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByText("Student account activated.")).toBeVisible();
  await expect(page.getByRole("cell", { name: "Active", exact: true })).toBeVisible();
  expect(writes).toEqual([false, true]);
  for (const width of [390, 768, 1440] as const) {
    await page.setViewportSize({ width, height: 900 });
    const overflow = await page.evaluate(
      () => document.documentElement.scrollWidth > window.innerWidth,
    );
    expect(overflow).toBe(false);
  }
});

test("supervisor capacity validates the assigned floor and refreshes after success", async ({
  page,
}) => {
  let capacity = sampleSupervisor.maxProjects;
  let updates = 0;
  await page.route(origin + "/api/Admin/Students?**", (route) =>
    route.fulfill({ json: listing([]) }),
  );
  await page.route(/^https:\/\/capnexa\.test\/api\/Admin\/Supervisors(?:[/?]|$)/, async (route) => {
    const request = route.request();
    const path = new URL(request.url()).pathname;
    expect(request.headers().authorization).toBe("Bearer fixture-admin");
    if (path.endsWith("/max-projects")) {
      expect(request.method()).toBe("PUT");
      expect(request.postDataJSON()).toEqual({ maxProjects: 4 });
      capacity = 4;
      updates++;
    }
    const item = { ...sampleSupervisor, maxProjects: capacity, availableCapacity: capacity - 1 };
    const detail: AdminSupervisorDetailDto = {
      ...item,
      assignedProjects: [
        {
          projectId: 26,
          title: "Campus Atlas",
          status: "Active",
          teamId: 26,
          teamName: "Atlas",
          teamCode: "TEAM-26",
        },
      ],
    };
    await route.fulfill({ json: path.endsWith("/Supervisors") ? listing([item]) : detail });
  });
  await login(page);
  await page.getByRole("link", { name: "Supervisors", exact: true }).click();
  await page.getByRole("button", { name: "Olivia Bennett", exact: true }).click();
  await expect(page.getByRole("heading", { name: "Campus Atlas" })).toBeVisible();
  await page.getByRole("button", { name: "Maximum projects", exact: true }).click();
  const input = page.getByRole("spinbutton", { name: "Maximum projects" });
  await expect(input).toHaveValue("3");
  await input.fill("0");
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByRole("dialog")).toBeVisible();
  expect(updates).toBe(0);
  await input.fill("4");
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByText("Supervisor capacity updated.")).toBeVisible();
  await expect(page.getByRole("cell", { name: "1 / 4" })).toBeVisible();
  expect(updates).toBe(1);
});

test("proposal approval refreshes pending records and links to assignment", async ({ page }) => {
  let approved = false;
  await page.route(origin + "/api/Admin/Students?**", (route) =>
    route.fulfill({ json: listing([]) }),
  );
  await page.route(origin + "/api/Admin/ProjectAssignments/unassigned?**", (route) =>
    route.fulfill({ json: listing([]) }),
  );
  await page.route(
    /^https:\/\/capnexa\.test\/api\/Admin\/ProjectProposals(?:[/?]|$)/,
    async (route) => {
      const request = route.request();
      const url = new URL(request.url());
      expect(request.headers().authorization).toBe("Bearer fixture-admin");
      if (url.pathname.endsWith("/review")) {
        expect(request.method()).toBe("PUT");
        expect(request.postDataJSON()).toEqual({ decision: "Approved", reason: null });
        approved = true;
      }
      const detail: AdminProposalDetailDto = {
        projectId: proposal.projectId,
        title: proposal.title,
        description: "Study-space availability",
        status: approved ? "Approved" : "PendingReview",
        createdAt: date,
        submittedAt: date,
        team: { ...team, maxMembers: 4, members: [] },
        review: approved
          ? {
              decision: "Approved",
              reason: null,
              reviewedByUserId: "reviewer",
              reviewedByName: "Fixture Admin",
              reviewedAt: date,
            }
          : null,
      };
      await route.fulfill({
        json: url.pathname.endsWith("/ProjectProposals")
          ? listing(approved ? [] : [proposal])
          : detail,
      });
    },
  );
  await login(page);
  await page.getByRole("link", { name: "Proposals", exact: true }).click();
  await page.getByRole("button", { name: "Library Lens" }).click();
  await expect(page.getByText("Study-space availability", { exact: true })).toBeVisible();
  await page.getByRole("button", { name: "Approve proposal" }).click();
  await page.getByRole("button", { name: "Confirm change" }).click();
  await expect(page.getByText("No matching records.")).toBeVisible();
  await page.getByRole("link", { name: "Supervisor assignments" }).click();
  await expect(page).toHaveURL(/\/admin\/assignments$/);
  expect(approved).toBe(true);
});

test("Supervisor login cannot open Admin routes", async ({ page }) => {
  await login(page, { ...auth, user: { ...auth.user, roles: ["Supervisor"] } });
  await expect(page).toHaveURL(/\/supervisor\/profile$/);
  await page.evaluate(() => {
    history.pushState(null, "", "/admin/students");
    window.dispatchEvent(new PopStateEvent("popstate"));
  });
  await expect(page).toHaveURL(/\/forbidden$/);
});
