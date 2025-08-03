#!/usr/bin/env node

/**
 * Modifications Table Generator
 * 
 * This script parses all modification files in the modifications folder
 * and generates a comprehensive table grouped by rarity with pricing information.
 * 
 * Usage: node generate-modifications-table.js
 * Output: modifications-table.md
 */

const fs = require('fs');
const path = require('path');

// Configuration
const MODIFICATIONS_FOLDER = '../modifications';
const OUTPUT_FILE = 'modifications-table.md';

// Rarity configuration from StoreItem.cs
const RARITY_CONFIG = {
    'Common': { multiplier: 1.0, color: '🟢' },
    'Uncommon': { multiplier: 1.2, color: '🔵' },
    'Rare': { multiplier: 1.5, color: '🟣' },
    'Epic': { multiplier: 2.0, color: '🟠' },
    'Legendary': { multiplier: 3.0, color: '🟡' }
};

/**
 * Extracts modification data from a C# file
 * @param {string} filePath - Path to the modification file
 * @returns {Object|null} - Modification data or null if parsing fails
 */
function parseModificationFile(filePath) {
    try {
        const content = fs.readFileSync(filePath, 'utf8');
        const fileName = path.basename(filePath, '.cs');
        
        // Extract class name
        const classMatch = content.match(/public class (\w+)/);
        const className = classMatch ? classMatch[1] : fileName;
        
        // Extract Name property
        const nameMatch = content.match(/public override string Name => "([^"]+)"/);
        const name = nameMatch ? nameMatch[1] : 'Unknown';
        
        // Extract Description property
        const descriptionMatch = content.match(/public override string Description => "([^"]+)"/);
        const description = descriptionMatch ? descriptionMatch[1] : 'No description';
        
        // Extract Rarity property
        const rarityMatch = content.match(/public override Rarity Rarity => Rarity\.(\w+)/);
        const rarity = rarityMatch ? rarityMatch[1] : 'Common';
        
        // Extract Material property
        const materialMatch = content.match(/public override Material Material => Resources\.Load<Material>\("([^"]+)"\)/);
        const material = materialMatch ? materialMatch[1] : 'Unknown';
        
        // Check if it modifies damage
        const isNotModifyingDamage = content.includes('public override bool IsNotModifyingDamage => true');
        
        // Try to determine modification type from various methods
        let effectType = 'Unknown';
        if (content.includes('GetModifiedValue')) effectType = 'Stat Modifier';
        if (content.includes('GetProjectileCount')) effectType = 'Projectile Modifier';
        if (content.includes('ModifyIncomingDamage')) effectType = 'Damage Modifier';
        if (content.includes('ApplyOnShoot')) effectType = 'Shoot Effect';
        if (content.includes('ApplyOnKill')) effectType = 'Kill Effect';
        if (content.includes('ApplyOnUpdate')) effectType = 'Passive Effect';
        if (content.includes('ApplyOnTakeDamage')) effectType = 'Damage Reaction';
        
        // Check if it's commented out or disabled
        const isDisabled = content.includes('// not implemented') || 
                          content.includes('// buggy') || 
                          content.includes('// turns out too OP');
        
        return {
            fileName: fileName,
            className: className,
            name: name,
            description: description,
            rarity: rarity,
            material: material,
            effectType: effectType,
            isNotModifyingDamage: isNotModifyingDamage,
            isDisabled: isDisabled,
            filePath: filePath
        };
    } catch (error) {
        console.error(`Error parsing ${filePath}:`, error.message);
        return null;
    }
}

/**
 * Gets all modification files from the modifications folder
 * @returns {Array} - Array of file paths
 */
function getModificationFiles() {
    const modificationsPath = path.join(__dirname, MODIFICATIONS_FOLDER);
    
    if (!fs.existsSync(modificationsPath)) {
        console.error(`Modifications folder not found: ${modificationsPath}`);
        return [];
    }
    
    return fs.readdirSync(modificationsPath)
        .filter(file => file.endsWith('.cs'))
        .map(file => path.join(modificationsPath, file));
}

/**
 * Groups modifications by rarity
 * @param {Array} modifications - Array of modification objects
 * @returns {Object} - Object with rarity as keys and arrays of modifications as values
 */
function groupByRarity(modifications) {
    const grouped = {};
    
    // Initialize all rarity groups
    Object.keys(RARITY_CONFIG).forEach(rarity => {
        grouped[rarity] = [];
    });
    
    modifications.forEach(mod => {
        if (grouped[mod.rarity]) {
            grouped[mod.rarity].push(mod);
        } else {
            // Handle unknown rarities
            if (!grouped['Unknown']) {
                grouped['Unknown'] = [];
            }
            grouped['Unknown'].push(mod);
        }
    });
    
    return grouped;
}

/**
 * Generates the markdown table
 * @param {Object} groupedMods - Modifications grouped by rarity
 * @returns {string} - Markdown content
 */
function generateMarkdownTable(groupedMods) {
    let markdown = `# Modifications Table by Rarity

*Generated automatically from modification files*  
*Generation date: ${new Date().toISOString()}*

## Overview

This table shows all modifications in the game, grouped by rarity. The price multiplier affects the base shop price according to the rarity tier.

`;

    // Add rarity summary
    markdown += `## Rarity Tiers Summary

| Rarity | Icon | Price Multiplier | Count |\n`;
    markdown += `|--------|------|------------------|-------|\n`;
    
    Object.entries(RARITY_CONFIG).forEach(([rarity, config]) => {
        const count = groupedMods[rarity] ? groupedMods[rarity].length : 0;
        markdown += `| **${rarity}** | ${config.color} | ${config.multiplier}x | ${count} |\n`;
    });
    
    markdown += `\n`;

    // Add detailed tables for each rarity
    Object.entries(RARITY_CONFIG).forEach(([rarity, config]) => {
        const mods = groupedMods[rarity] || [];
        if (mods.length === 0) return;
        
        markdown += `## ${config.color} ${rarity} Modifications (${config.multiplier}x price)\n\n`;
        markdown += `| Name | Description | Effect Type | Material | Status |\n`;
        markdown += `|------|-------------|-------------|----------|--------|\n`;
        
        mods.forEach(mod => {
            const status = mod.isDisabled ? '❌ Disabled' : '✅ Active';
            const material = mod.material.replace('Materials/', '');
            
            markdown += `| **${mod.name}** | ${mod.description} | ${mod.effectType} | ${material} | ${status} |\n`;
        });
        
        markdown += `\n`;
    });

    // Add unknown rarity section if any
    if (groupedMods['Unknown'] && groupedMods['Unknown'].length > 0) {
        markdown += `## ❓ Unknown Rarity Modifications\n\n`;
        markdown += `| Name | Description | Effect Type | Material | Status |\n`;
        markdown += `|------|-------------|-------------|----------|--------|\n`;
        
        groupedMods['Unknown'].forEach(mod => {
            const status = mod.isDisabled ? '❌ Disabled' : '✅ Active';
            const material = mod.material.replace('Materials/', '');
            
            markdown += `| **${mod.name}** | ${mod.description} | ${mod.effectType} | ${material} | ${status} |\n`;
        });
        
        markdown += `\n`;
    }

    // Add statistics
    const totalMods = Object.values(groupedMods).flat().length;
    const activeMods = Object.values(groupedMods).flat().filter(mod => !mod.isDisabled).length;
    const disabledMods = totalMods - activeMods;

    markdown += `## Statistics

- **Total Modifications**: ${totalMods}
- **Active Modifications**: ${activeMods}
- **Disabled Modifications**: ${disabledMods}

### Distribution by Effect Type

`;

    // Count by effect type
    const effectTypeCounts = {};
    Object.values(groupedMods).flat().forEach(mod => {
        effectTypeCounts[mod.effectType] = (effectTypeCounts[mod.effectType] || 0) + 1;
    });

    Object.entries(effectTypeCounts)
        .sort(([,a], [,b]) => b - a)
        .forEach(([type, count]) => {
            markdown += `- **${type}**: ${count}\n`;
        });

    markdown += `\n## Notes

- Price multipliers are applied to the base shop price
- Disabled modifications are marked and may not appear in-game
- Effect types are determined by analyzing the modification's methods
- Material paths are from the Resources folder
- Some modifications may have multiple effect types but only the primary one is shown

---

*This file is auto-generated. Do not edit manually.*
*To regenerate, run: \`node generate-modifications-table.js\`*
`;

    return markdown;
}

/**
 * Main function
 */
function main() {
    console.log('🔍 Scanning modifications folder...');
    
    const modificationFiles = getModificationFiles();
    if (modificationFiles.length === 0) {
        console.error('❌ No modification files found!');
        return;
    }
    
    console.log(`📁 Found ${modificationFiles.length} modification files`);
    
    // Parse all modification files
    const modifications = [];
    modificationFiles.forEach(filePath => {
        const mod = parseModificationFile(filePath);
        if (mod) {
            modifications.push(mod);
            console.log(`✅ Parsed: ${mod.name} (${mod.rarity})`);
        }
    });
    
    console.log(`📊 Successfully parsed ${modifications.length} modifications`);
    
    // Group by rarity
    const groupedMods = groupByRarity(modifications);
    
    // Generate markdown
    const markdown = generateMarkdownTable(groupedMods);
    
    // Write to file
    const outputPath = path.join(__dirname, OUTPUT_FILE);
    fs.writeFileSync(outputPath, markdown, 'utf8');
    
    console.log(`📝 Generated table: ${outputPath}`);
    console.log('✨ Done!');
}

// Run the script
if (require.main === module) {
    main();
}

module.exports = {
    parseModificationFile,
    getModificationFiles,
    groupByRarity,
    generateMarkdownTable
};
