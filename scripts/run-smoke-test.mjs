import path from "node:path";
import { fileURLToPath } from "node:url";
import { ESLint } from "eslint";

const currentFilePath = fileURLToPath(import.meta.url);
const scriptsDirectoryPath = path.dirname(currentFilePath);
const repositoryRootPath = path.resolve(scriptsDirectoryPath, "..");
const fixtureConfigPath = path.join(
  repositoryRootPath,
  "tests/fixtures/sample-project/eslint.config.js",
);
const fixtureSourcePath = path.join(
  repositoryRootPath,
  "tests/fixtures/sample-project/sample.ts",
);

const eslint = new ESLint({
  overrideConfigFile: fixtureConfigPath,
});

const results = await eslint.lintFiles([fixtureSourcePath]);
const fixtureResult = results[0];

if (fixtureResult === undefined) {
  throw new Error("Smoke test did not produce an ESLint result.");
}

const ruleIds = fixtureResult.messages
  .map((message) => message.ruleId)
  .filter(Boolean);
const requiredRuleIds = [
  "@distrohelena/linter/declaration-spacing",
  "@distrohelena/linter/if-following-spacing",
];
const missingRuleIds = requiredRuleIds.filter(
  (ruleId) => !ruleIds.includes(ruleId),
);

if (missingRuleIds.length > 0) {
  throw new Error(
    `Smoke test did not report the expected Helena rules. Missing: ${missingRuleIds.join(", ")}. Received: ${ruleIds.join(", ")}`,
  );
}

console.log(`Smoke test detected Helena rules: ${requiredRuleIds.join(", ")}`);
