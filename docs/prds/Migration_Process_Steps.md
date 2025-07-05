# Cognitive Mesh Documentation Migration & Canonicalization Process

## Step-by-Step Process

1. **Preparation**
   - Identify the next file(s) to process from any of the following source locations:
     - `docs/prds/_more_prd/`
     - `docs/prds/_neural/`
     - Any dash-numbered PRD folders (e.g., `docs/prds/01-...`, `docs/prds/02-...`, ..., `docs/prds/09-...`).
   - Reference the Widget Master Matrix to determine the correct PRD directory (by Ecosystem/Category) and canonical filename (by Marketing Name).

2. **Read and Prepare Content**
   - Read the full content of the source file.
   - Prepare a complete YAML frontmatter block with all Widget Master Matrix fields, in the correct order and with canonical values.

3. **Migrate to PRD Directory**
   - Check for existing files with the same name in the target PRD directory.
   - If a file exists, increment a numeric suffix to avoid overwriting.
   - Write the new file with the correct frontmatter and full content.
   - **If the file represents a new widget or product for the ecosystem, add an entry to the corresponding `index.md` (Widget Master Matrix) in that PRD directory.**
   - **After adding the new entry to the index, use the frontmatter from that canonical entry for the migrated file.**
   - **Never overwrite or conflate UI/Widget PRDs with Backend PRDs (or vice versa) for the same product/module. Always preserve both as distinct files, with clear filenames and index entries. If both exist, append `UI`, `Widget`, or `Backend` as appropriate to the filename and "Marketing Name" in the frontmatter and index.**

4. **Archive the Original**
   - Check for existing archive files with the same name.
   - Archive the original file in `docs/archive/` with an `old_` prefix and numeric suffix if needed, preserving the full content and frontmatter.

5. **Remove the Source**
   - Delete the original file from its source location after successful migration and archival.

6. **Deduplicate and Canonicalize**
   - In the PRD directory, keep only the latest, most complete version, renaming it to the canonical filename (no suffix).
   - Delete any older or duplicate versions.
   - Optionally, update the archive to match the latest version, or keep the original as a record.
   - Ensure all files in the PRD directories are named according to the canonical "Marketing Name" from the Widget Master Matrix, using spaces, and only add a suffix if required for uniqueness.

7. **Track and Confirm Migration Status**
   - Before each batch, refer to the Widget Master Matrix migration tracker file.
   - After each batch, update the MigrationStatus column for each processed file (e.g., 'complete', 'semi', 'pending') **in the corresponding index.md (Widget Master Matrix) for that PRD directory**.
   - **After migration, confirm that each new file in the PRD directory contains the correct, complete, and canonical data for its ecosystem, matching the Widget Master Matrix. The file must not be empty or missing any required fields or rows. Document this confirmation in the migration tracker or process log.**
   - If any issues are found (e.g., missing fields, incomplete data, or mismatches), resolve them immediately and re-confirm.

8. **Continuous Improvement**
   - Periodically review the migration process for clarity, completeness, and efficiency.
   - Update these steps as new source locations, ecosystems, or requirements emerge.
   - Ensure all contributors are aware of the latest canonicalization and migration rules.

---

**Automation Rule:** Do not pause, wait, or prompt the user between migration steps; only prompt after a batch is complete or if an issue/error is encountered. After supplying feedback and receiving user acceptance (e.g., 'yes'), immediately continue with the migration process until the batch is complete, without pausing or prompting again until batch completion or an issue arises.

This process ensures all documentation is organized, deduplicated, and canonical, with historical versions preserved in the archive and migration status tracked for every widget. It now covers all relevant source folders and enforces strict confirmation and canonicalization for every migrated file. 
