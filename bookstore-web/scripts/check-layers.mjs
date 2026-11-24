// Fails if any import crosses a Clean Architecture boundary in src/app.
// Usage: npm run check:layers
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative, sep } from 'node:path';

const root = join(import.meta.dirname, '..', 'src', 'app');

/** What each layer must never import. Dependencies point inward: presentation -> application -> domain. */
const forbidden = {
  domain: [/^@angular\//, /^rxjs/, /^primeng/, /^@primeuix/, /^@application\//, /^@infrastructure\//, /^@presentation\//],
  application: [/^@angular\/common\/http/, /^primeng/, /^@infrastructure\//, /^@presentation\//],
  infrastructure: [/^primeng/, /^@application\//, /^@presentation\//],
  presentation: [/^@angular\/common\/http/, /^@infrastructure\//],
};
const layerOf = (file) => relative(root, file).split(sep)[0];

const files = (dir) =>
  readdirSync(dir).flatMap((name) => {
    const path = join(dir, name);
    return statSync(path).isDirectory() ? files(path) : path.endsWith('.ts') && !path.endsWith('.spec.ts') ? [path] : [];
  });

const violations = [];
for (const file of files(root)) {
  const layer = layerOf(file);
  const rules = forbidden[layer];
  if (!rules) continue; // composition root (app.ts, app.config.ts) may wire any layer
  for (const [, specifier] of readFileSync(file, 'utf8').matchAll(/^\s*import\s[^'"]*['"]([^'"]+)['"]/gm)) {
    const crossesLayer = specifier.startsWith('.') && !relative(root, join(file, '..', specifier)).startsWith(layer);
    if (rules.some((rule) => rule.test(specifier)) || crossesLayer) {
      violations.push(`${relative(root, file)} (${layer}) imports '${specifier}'`);
    }
  }
}

if (violations.length) {
  console.error(`Layer violations:\n  ${violations.join('\n  ')}`);
  process.exit(1);
}
console.log('Layer boundaries OK.');
