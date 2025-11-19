import {beforeEach, describe, test} from 'node:test';
import assert from 'node:assert'
import { WebAPIClient, type APIClient} from './client.js';
var client: APIClient;

describe('WebAPI client', () => {
  beforeEach(async () => client = new WebAPIClient("http://localhost:5233"));
  
  describe('logged out', () => {
    test('not authenticated', () => {
       assert.strictEqual(client.isAuthenticated(), false)
    });
    test('login with invalid token', () => {
      assert.rejects(client.authenticate("invalid token string"))
    });

    test('login with username and password', () => {
      assert.doesNotReject(client.login("admin", "abc12345"));
      assert.strictEqual(client.isAuthenticated(), true);
    });
/*
    test('login with username and password', () => {
      assert.doesNotReject(client.login("invalid", "invalidpassword123"))
      expect(client.isAuthenticated()).toBe(false);
    });

    test('set token directly (unchecked)', () => {
      client.authenticateUnchecked("my token")
      expect(client.isAuthenticated()).toBe(true);
    });

    test('logout removes token', () => {
      client.authenticateUnchecked("my token")
      client.logout(false)
      expect(client.isAuthenticated()).toBe(false);
    });*/
  });
});