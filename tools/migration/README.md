# PRD Migration Tool

This **PRD Migration Tool** helps canonicalize and migrate PRDs across the monorepo, ensuring a clean and consistent structure. 

## Project Structure

```plaintext
cognitive-mesh/
├── docs/
│   └── prds/                # Canonicalized PRDs
├── tools/
│   └── migration/           # Migration tool
│       ├── migrate_prds.py  # <— The Migration Script
│       ├── pyproject.toml   # Poetry minimal project configuration
│       └── README.md         # This file
├── .github/
│   └── workflows/
│       └── migrate.yml      # CI dry-run + linter workflow
└── Makefile                 # `make migrate batch=25`
```

## Features

- **Batch Processing**: Migrate PRDs in batches using the `batch` argument.
- **Dry-run Support**: Validate migrations before applying using the `--dry-run` option.
- **Duplicate/Index Validation**: Automatically detect issues like duplicates or indexing errors.

## Setup

1. Install [Poetry](https://python-poetry.org/):
   ```bash
   pip install poetry
   ```
2. Navigate to the `tools/migration` directory and install dependencies:
   ```bash
   cd tools/migration
   poetry install
   ```

## Usage

### Canonicalize PRDs

Run the migration script directly:
```bash
poetry run python migrate_prds.py --batch 25
```

Run a dry-run to validate results:
50 | ```bash
51 | poetry run python migrate_prds.py --batch 25 --dry-run
52 | ```
53 | 
54 | ### Virtual Environment Workspace
55 | 
56 | To ensure clean development and dependency isolation for `migrate_prds.py`:
57 | 
58 | 1. **Create a Virtual Environment**:
59 |    ```bash
60 |    python -m venv .venv
61 |    source .venv/bin/activate      # Windows: .venv\\Scripts\\activate
62 |    ```
63 | 
64 | 2. **Install Dependencies**:
65 |    Install required packages. For batch processing or GPU optimization:
66 |    ```bash
67 |    pip install -r requirements.txt
68 |    ```
69 | 
70 | Note: CUDA GPU environments need relevant parallel CUDA dependencies/tools!
       Debian_WARN_off_GPU

## Editing details comp=>CUDA prodHANDL pipeline

       GPUs/q-debs Install_mechan sandvelop_funcTIONAL?
```bash
poetry run python migrate_prds.py --batch 25 --dry-run
```

### Makefile Shortcut
Use the Makefile to simplify migration commands:
```bash
make migrate batch=25
```

## CI Integration

A GitHub Actions workflow is provided to perform a `dry-run` and ensure all migrations pass before merging pull requests.

### CI Workflow File: `.github/workflows/migrate.yml`

```yaml
name: PRD Migration Check
on: [pull_request]
jobs:
  migrate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with: { python-version: "3.12" }
      - run: pip install poetry
      - run: poetry install
        working-directory: tools/migration
      - run: poetry run python migrate_prds.py --batch 25 --dry-run
        working-directory: tools/migration
```

Pull requests will fail if the script finds issues such as:
- Duplicate PRDs
- Indexing errors

## Notes

- Keeping the tool as a standalone Poetry project ensures that the rest of the monorepo does not inherit unnecessary dependencies.
- Review output logs for details on any detected issues.
# Migration Setup Guide

This guide covers the setup and prerequisites for running the PRD migration script effectively.

## 1. Install Essentials

### Steps

1. **Create and activate your virtual environment** (if not already set up):
   ```bash
   python -m venv .venv
   source .venv/bin/activate      # For Windows: .venv\\Scripts\\activate
   ```

2. **Install core libraries**:
   ```bash
   pip install -U sentence-transformers torch
   ```
   - **Tip**: For NVIDIA GPUs, ensure the matching CUDA toolkit (12.x for the latest PyTorch wheels) is installed. This ensures CUDA integration for faster execution.

---

## 2. Pre-Download Model Weights (One-Liner)

Pre-fetch the necessary model weights to avoid latency during script runtime.

```bash
python - <<'PY'
from sentence_transformers import SentenceTransformer
SentenceTransformer("BAAI/bge-large-en-v1.5", cache_folder=".huggingface")
print("✓ Model downloaded to", SentenceTransformer._get_default_cache_path())
PY
```

- **What happens**:
  - The first execution fetches the 1.9 GB FP16 checkpoint + tokenizer via Hugging Face’s API.
  - Downloads are stored in the default cache directory (`~/.cache/huggingface/`) or the specified `cache_folder`.
  - Subsequent script executions memory-map the local copy, avoiding network dependencies.

---

## 3. (Alternative) Use huggingface-cli for Plain Downloads

For those preferring CLI-based downloads:

1. **Install Hugging Face Hub**:
   ```bash
   pip install -U huggingface_hub
   ```
2. **Download the model**:
   ```bash
   huggingface-cli download BAAI/bge-large-en-v1.5 --local-dir bge-large --local-dir-use-symlinks False
   ```
3. **Point the script to the downloaded checkpoint**:
   ```bash
   python migrate_prds.py --checkpoint ./bge-large
   ```

---

## 4. Verify GPU and Encoding Speeds

Evaluate GPU/CPU performance to ensure your environment is optimized:

```bash
python - <<'PY'
from sentence_transformers import SentenceTransformer, util, models
import torch, time
model = SentenceTransformer("BAAI/bge-large-en-v1.5")
start = time.time()
emb = model.encode(["Test sentence"] * 1024, batch_size=64, show_progress_bar=False)
print(f"{len(emb)} vectors in {time.time()-start:.2f}s (device={model.device})")
PY
```

### Expected Results:
- **GPU**: ~2,500 sentences/second on an RTX 500.
- **CPU**: ~120 sentences/second.

---

Follow this guide for seamless PRD migration with improved performance and pre-configured dependencies.