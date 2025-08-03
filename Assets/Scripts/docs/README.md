# Documentation Tools

This folder contains automated tools for generating documentation from the codebase.

## Modifications Table Generator

### Overview
The `generate-modifications-table.js` script automatically scans all modification files and generates a comprehensive table grouped by rarity.

### Usage

```bash
# Using Node.js directly
node generate-modifications-table.js

# Using npm scripts
npm run generate
# or
npm start
```

### Output
The script generates `modifications-table.md` with:
- Complete list of all modifications
- Grouped by rarity (Common, Uncommon, Rare, Epic, Legendary)
- Price multipliers for each rarity
- Effect types and descriptions
- Material assignments
- Status (Active/Disabled)
- Statistics and distribution analysis

### Features
- **Automatic parsing**: Reads C# modification files directly
- **Smart analysis**: Determines effect types by analyzing methods
- **Rarity detection**: Extracts rarity from code
- **Status checking**: Identifies disabled modifications
- **Material mapping**: Shows which visual materials are used
- **Statistics**: Provides counts and distribution data
- **Markdown output**: Clean, readable table format

### File Structure
```
docs/
├── generate-modifications-table.js   # Main generator script
├── modifications-table.md            # Generated output (auto-updated)
├── package.json                      # Node.js project config
└── README.md                         # This file
```

### Customization
You can modify the script to:
- Change output format
- Add new analysis features
- Modify rarity configurations
- Include additional metadata
- Generate different file formats

### Dependencies
- Node.js (no external dependencies required)
- Access to the modifications folder (`../modifications`)

### Regeneration
The table is automatically regenerated each time you run the script. The generated file includes a timestamp and should not be edited manually.

---

*For more information about the modification system, see the ModificationSystemExtensionGuide.md*
