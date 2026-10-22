"""Build the distributable UPM directory from the project's canonical sources.

Run with Python 3: python Tools/rebuild_upm.py [--check]
The UPM folder is generated. Edit Assets/Scripts, then run this tool again.
"""
from pathlib import Path
import argparse
import hashlib
import json
import uuid

ROOT = Path(__file__).resolve().parents[1]
PACKAGE = ROOT / "UPM/com.guisha.framework"
VERSION = "1.0.1"


def collect():
    files = {}
    for source, target in [
        ("Assets/Scripts", "Runtime/Scripts"),
        ("Assets/Shaders", "Runtime/Shaders"),
        ("Assets/Tests/EditMode", "Tests/Editor"),
        ("Assets/Tests/PlayMode", "Tests/Runtime"),
    ]:
        for path in sorted((ROOT / source).rglob("*")):
            if path.is_file():
                relative = path.relative_to(ROOT / source)
                if source == "Assets/Scripts" and relative.parts[0] == "Editor":
                    destination = "Editor/" + relative.as_posix().removeprefix("Editor/")
                elif source == "Assets/Scripts" and relative.as_posix() == "Editor.meta":
                    continue
                else:
                    destination = target + "/" + relative.as_posix()
                text = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n")
                if path.suffix == ".meta":
                    text = "\n".join(line.rstrip() for line in text.splitlines()).rstrip() + "\n"
                files[destination] = text.encode("utf8")
    manifest = {
        "name": "com.guisha.framework",
        "version": VERSION,
        "displayName": "Guisha Unity Framework",
        "description": "Unity C# framework with Core utilities, 23 GoF patterns, gameplay, animation and optional Built-in/URP/HDRP adapters.",
        "unity": "2022.3",
        "author": {"name": "12121-doy"},
        "dependencies": {
            "com.unity.addressables": "1.22.3",
            "com.unity.ugui": "1.0.0",
            "com.unity.test-framework": "1.1.33",
            **{"com.unity.modules." + name: "1.0.0" for name in [
                "ai", "animation", "audio", "director", "imgui", "jsonserialize",
                "particlesystem", "physics", "ui",
            ]},
        },
        "documentationUrl": "https://github.com/12121-doy/UnityTemplate/blob/main/Docs/Guisha/UPM安装说明.md",
    }
    files["package.json"] = (json.dumps(manifest, indent=2, ensure_ascii=False) + "\n").encode("utf8")
    files["README.md"] = (ROOT / "Docs/Guisha/UPM安装说明.md").read_text(encoding="utf8").replace("\r\n", "\n").encode("utf8")
    files["Documentation~/index.md"] = files["README.md"]
    files["CHANGELOG.md"] = ("# Changelog\n\n## 1.0.1 - 2026-10-22\n\n"
        "- Fixed AnimatorController parameters lost when first generating sample assets.\n"
        "- Added a forced-reimport regression test for generated animation parameters and states.\n\n"
        "## 1.0.0 - 2026-10-22\n\n"
        "- Added an installable UPM entry point with declared Unity package dependencies.\n"
        "- Includes the current Chinese-commented framework and three rendering adapters.\n"
        "- Gameplay bootstrap is opt-in through a project-owned FrameworkSettings asset.\n").encode("utf8")
    folders = set()
    for relative in list(files):
        parent = Path(relative).parent
        while str(parent) != ".":
            if not any(part.endswith("~") for part in parent.parts):
                folders.add(parent.as_posix())
            parent = parent.parent
    for folder in sorted(folders):
        meta = folder + ".meta"
        if meta not in files:
            guid = uuid.uuid5(uuid.NAMESPACE_URL, "guisha-upm/" + folder).hex
            files[meta] = ("fileFormatVersion: 2\nguid: " + guid + "\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n").encode("utf8")
    for relative in list(files):
        if relative.endswith(".meta") or "~" in relative or relative + ".meta" in files:
            continue
        guid = uuid.uuid5(uuid.NAMESPACE_URL, "guisha-upm/" + relative).hex
        files[relative + ".meta"] = ("fileFormatVersion: 2\nguid: " + guid + "\n").encode("utf8")
    return files


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true")
    check = parser.parse_args().check
    files = collect()
    record = PACKAGE / ".generated-files.json"
    previous = json.loads(record.read_text(encoding="utf8")) if record.exists() else {}
    changed = []
    for relative, data in files.items():
        path = PACKAGE / relative
        if not path.exists() or path.read_bytes() != data:
            changed.append(relative)
            if not check:
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_bytes(data)
    for relative, digest in previous.items():
        if relative in files:
            continue
        path = PACKAGE / relative
        if not path.resolve().is_relative_to(PACKAGE.resolve()):
            raise RuntimeError("Unsafe generated path: " + relative)
        if path.exists():
            if hashlib.sha256(path.read_bytes()).hexdigest() != digest:
                raise RuntimeError("Refuse to delete modified generated file: " + relative)
            changed.append(relative)
            if not check:
                path.unlink()
    if check:
        if changed:
            raise SystemExit("UPM files out of date: " + ", ".join(changed))
    else:
        record.write_text(json.dumps({p: hashlib.sha256(b).hexdigest() for p, b in sorted(files.items())}, indent=2) + "\n", encoding="utf8")
    print("UPM package:", len(files), "files;", len(changed), "updated; generated sources match Assets.")


if __name__ == "__main__":
    main()
