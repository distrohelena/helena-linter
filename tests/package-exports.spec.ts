import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";

const packageJsonPath = path.resolve(import.meta.dirname, "../package.json");
const packageJson = JSON.parse(readFileSync(packageJsonPath, "utf8")) as {
  exports?: Record<string, unknown>;
};

describe("package subpath exports", () => {
  it("publishes explicit root-level rule subpaths", () => {
    expect(packageJson.exports).toMatchObject({
      "./declaration-spacing": expect.any(Object),
      "./early-return": expect.any(Object),
      "./if-else-if-chain": expect.any(Object),
      "./if-following-spacing": expect.any(Object),
      "./redundant-else-if": expect.any(Object),
    });
  });
});
