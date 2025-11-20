import { beforeEach, describe, test } from "node:test";
import assert from "node:assert";
import { WebAPIClient, type APIClient } from "./client.ts";
import { getBackendSecrets } from "./secrets.ts"

var client: APIClient;

var secrets: {
  AdminPassword: string
};

describe("WebAPI client", () => {
  beforeEach(async () => {
    client = new WebAPIClient("http://localhost:5233");
    secrets = await getBackendSecrets() as typeof secrets; // TODO: validate
  });

  describe("logged out", () => {
    test("not authenticated", () => {
      assert.strictEqual(client.isAuthenticated(), false);
    });
    test("login with invalid token", () => {
      assert.rejects(client.authenticate("invalid token string"));
    });

    test("login with valid username and password", async () => {
      await assert.doesNotReject(client.login("admin", secrets.AdminPassword));
      assert.strictEqual(client.isAuthenticated(), true);
    });

    test("login with invalid username and password", async () => {
      await assert.rejects(client.login("invalid", "invalidpassword123"));
      assert.strictEqual(client.isAuthenticated(), false);
    });

    test("set token directly (unchecked)", () => {
      client.authenticateUnchecked("my token");
      assert.strictEqual(client.isAuthenticated(), true);
    });

    test("logout (untracked)", async () => {
      client.authenticateUnchecked("my token");
      await assert.doesNotReject(client.logout(false));
      assert.strictEqual(client.isAuthenticated(), false);
    });
  });
});
