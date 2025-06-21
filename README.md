# Emitter UI

A responsive web app for emitting data to a WebSocket or HTTP endpoint. Built with React, Tailwind CSS, and Vite.

## Features

- **Button Events**: Large on-screen button that detects pressed, held, and released states.
- **Mini Map XY Coordinates**: Interactive minimap for selecting X/Y coordinates, with manual input.
- **Extensible**: Easy to add new input elements and emit types.

## Installation

1. Clone the repository:
   ```sh
   git clone <your-repo-url>
   cd emitter-golf-codex
   ```

2. Install dependencies:
   ```sh
   npm install
   ```

## Development

Run the app locally:
```sh
npm run dev
```

## Deployment to GitHub Pages

1. Build the app:
   ```sh
   npm run build
   ```

2. Deploy the contents of the `dist/` folder to your GitHub Pages branch (usually `gh-pages`).

   - If you haven't set up GitHub Pages yet, go to your repository settings, scroll down to the "GitHub Pages" section, and select the branch you want to deploy (e.g., `gh-pages`).
   - When using GitHub Actions, make sure the workflow exports `GH_TOKEN` from `secrets.GITHUB_TOKEN` so the `gh-pages` CLI can authenticate.

3. Your app will be available at `https://<your-username>.github.io/<your-repo-name>/`.

## Project Structure

- `src/`: Source code
  - `components/`: React components
  - `App.jsx`: Main app component
  - `emitEvent.js`: Logic for emitting events
  - `main.jsx`: Entry point
  - `index.css`: Tailwind CSS entry

- `dist/`: Build output (deploy to GitHub Pages)

## License

MIT
