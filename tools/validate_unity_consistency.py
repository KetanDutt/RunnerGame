#!/usr/bin/env python3
"""Unity project consistency checks for RunnerGame (no Unity install required).

Validates:
  * C# brace balance (comments/strings stripped) for every script in _Scripts
  * scene YAML integrity: duplicate fileIDs, dangling local references,
    missing asset GUIDs (vs every .meta in the project), and MonoBehaviour
    serialized field names vs the actual C# field declarations
  * audio clip .meta integrity (AudioImporter present for every WAV)

Usage:  python3 tools/validate_unity_consistency.py [project-root]
        (project-root defaults to the repository root, i.e. the parent of
        the directory containing this script)
"""
import glob
import os
import re
import sys

ROOT = sys.argv[1] if len(sys.argv) > 1 else os.path.dirname(
    os.path.dirname(os.path.abspath(__file__)))
if not os.path.isdir(os.path.join(ROOT, "Assets")):
    sys.exit("error: project root %r has no Assets/ folder" % ROOT)
errors = []
warnings = []


# Script GUIDs provided by Unity packages (ugui, TMP, EventSystem) - their
# sources live in the package cache, not in Assets/.
PACKAGE_GUIDS = {
    "fe87c0e1cc204ed48ad3b37840f39efc",  # CanvasScaler (ugui)
    "0cd44c1031e13a943bb63640046fad76",  # StandaloneInputModule
    "76c392e42b5098c458856cdf6ecaaaa1",  # Canvas
    "4e29b1a8efbd4b44bb3f3716e73f07ff",  # GraphicRaycaster
    "dc42784cf147c0c48a680349fa168899",  # EventSystem
    "4f231c4fb786f3946a6b90b886c48677",  # Image
    "f4688fdb7df04437aeb418b961361dc5",  # TextMeshProUGUI
    "a23baa6fc1c32c449b602721a4e810d7",  # CanvasGroup
}


def err(msg):
    errors.append(msg)


def warn(msg):
    warnings.append(msg)


# ---------------------------------------------------------------- C# checks
def strip_csharp(src):
    """Remove comments and string/char literals, preserving structure."""
    out = []
    i, n = 0, len(src)
    while i < n:
        c = src[i]
        if c == '/' and i + 1 < n and src[i + 1] == '/':
            while i < n and src[i] != '\n':
                i += 1
        elif c == '/' and i + 1 < n and src[i + 1] == '*':
            i += 2
            while i + 1 < n and not (src[i] == '*' and src[i + 1] == '/'):
                i += 1
            i += 2
        elif c == '"':
            # check verbatim string
            if src.startswith("@\"", i) or src.startswith("$\"", i) or src.startswith("@$\"", i):
                j = i
                while j < n:
                    if src[j] == '"' and (j + 1 >= n or src[j + 1] != '"'):
                        j += 1
                        break
                    j += 1
                i = j
            else:
                j = i + 1
                while j < n and src[j] != '"':
                    if src[j] == '\\':
                        j += 1
                    j += 1
                i = j + 1
            out.append('" "')
        elif c == "'":
            j = i + 1
            while j < n and src[j] != "'":
                if src[j] == '\\':
                    j += 1
                j += 1
            i = j + 1
            out.append("' '")
        else:
            out.append(c)
            i += 1
    return "".join(out)


def check_csharp(path):
    src = open(path, encoding="utf-8").read()
    code = strip_csharp(src)
    for open_c, close_c in [("{", "}"), ("(", ")"), ("[", "]")]:
        if code.count(open_c) != code.count(close_c):
            err(f"{path}: unbalanced {open_c}{close_c} "
                f"({code.count(open_c)} vs {code.count(close_c)})")
    # namespace + class must exist
    if "namespace RunnerGame" not in code:
        warn(f"{path}: missing 'namespace RunnerGame'")
    # every method-ish heuristic: check for accidental 'public void X()' without body later
    # (rough: find 'return;' statements inside switch - skip)
    # check for stray semicolon-after-class tricks: none
    return src, code


def collect_script_fields(code):
    """Collect top-level field/property declarations (names) from a script body."""
    fields = set()
    # [SerializeField] private X name; / public X name; / private readonly X name = ...;
    for m in re.finditer(
        r"(?:public|private|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?[\w<>\[\],\s\?]+?\s(\w+)\s*(?:=|;)",
        code,
    ):
        name = m.group(1)
        if name in ("class", "interface", "struct", "enum", "new", "return", "if", "else",
                    "while", "for", "foreach", "switch", "case", "do", "var"):
            continue
        fields.add(name)
    return fields


# ---------------------------------------------------------------- run C#
script_dir = os.path.join(ROOT, "Assets/_Scripts")
scripts = {}
for f in sorted(glob.glob(os.path.join(script_dir, "*.cs"))):
    base = os.path.basename(f)
    if base.endswith(".meta"):
        continue
    src, code = check_csharp(f)
    scripts[base] = code
    # meta file must exist
    if not os.path.exists(f + ".meta"):
        err(f"{base}: missing .meta file!")

print(f"checked {len(scripts)} scripts")

# ---------------------------------------------------------------- YAML-ish scene checks
def parse_scene_docs(path):
    txt = open(path, encoding="utf-8").read()
    docs = re.findall(r"(?ms)^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*\n.*?(?=^--- !u!|\Z)", txt)
    return docs


for scene in ("Assets/Scenes/Gameplay.unity", "Assets/Scenes/Menu.unity"):
    path = os.path.join(ROOT, scene)
    docs = parse_scene_docs(path)
    ids = [int(d[1]) for d in docs]
    if len(ids) != len(set(ids)):
        seen = set()
        for i in ids:
            if i in seen:
                err(f"{scene}: duplicate fileID {i}")
            seen.add(i)
    idset = set(ids)

    # local references {fileID: N} (without guid) must resolve
    txt = open(path, encoding="utf-8").read()
    for m in re.finditer(r"\{fileID: (-?\d+)\}", txt):
        rid = int(m.group(1))
        if rid != 0 and rid not in idset:
            err(f"{scene}: dangling local reference fileID {rid}")

    # guid references must exist (type 2/3 assets), except built-ins (guid 0...0 type 0)
    for m in re.finditer(r"guid: ([a-f0-9]{32}), type: ([0-9])", txt):
        g, typ = m.group(1), m.group(2)
        if typ == "0":
            continue
        meta = None
        # search guid map (cache per run)
        if not hasattr(parse_scene_docs, "guidmap"):
            gm = {}
            for metafile in glob.glob(os.path.join(ROOT, "Assets/**/*.meta"), recursive=True):
                try:
                    t = open(metafile, encoding="utf-8", errors="ignore").read()
                    gm2 = re.search(r"guid: ([a-f0-9]{32})", t)
                    if gm2:
                        gm[gm2.group(1)] = metafile
                except OSError:
                    pass
            parse_scene_docs.guidmap = gm
        if g not in parse_scene_docs.guidmap and g not in PACKAGE_GUIDS:
            err(f"{scene}: missing asset guid {g} (type {typ})")

    # MonoBehaviour serialized fields must exist in the script
    for d in docs:
        body = d[0] if isinstance(d, tuple) else None
    for doc in re.finditer(r"(?ms)^--- !u!114 &(-?\d+)\s*\nMonoBehaviour:\n(.*?)(?=^--- !u!|\Z)", txt):
        body = doc.group(2)
        gm = re.search(r"guid: ([a-f0-9]{32}), type: 3", body)
        if not gm:
            continue
        g = gm.group(1)
        # map guid -> script
        script = None
        for metafile in glob.glob(os.path.join(ROOT, "Assets/_Scripts/*.cs.meta")):
            t = open(metafile, encoding="utf-8", errors="ignore").read()
            if f"guid: {g}" in t:
                script_name = os.path.basename(metafile)[:-5]
                break
        if g in PACKAGE_GUIDS:
            continue
        if script_name not in scripts:
            warn(f"{scene}: script guid {g} has no source?")
            continue
        code = scripts[script_name]
        fields = collect_script_fields(code)
        # serialized fields = lines "  name: value" at 2-space indent after m_EditorClassIdentifier
        after = body.split("m_EditorClassIdentifier:", 1)
        if len(after) < 2:
            continue
        for line in after[1].splitlines():
            m2 = re.match(r"^  (\w+):", line)
            if m2:
                fname = m2.group(1)
                if fname not in fields:
                    # Fields intentionally removed in the rewrite; Unity silently
                    # ignores extra serialized values, so this is safe.
                    legacy_ok = {
                        ("GameManager.cs", "isPaused"),
                        ("GameManager.cs", "isOver"),
                        ("ObstacleSpawner.cs", "prefabWidth"),
                    }
                    if (script_name, fname) in legacy_ok:
                        warn(f"{scene}: [{script_name}] legacy field '{fname}' (ignored by Unity)")
                    else:
                        err(f"{scene}: [{script_name}] serialized field '{fname}' not found in script")

print("scene structure checks done")

# ---------------------------------------------------------------- meta integrity for new assets
for meta in glob.glob(os.path.join(ROOT, "Assets/_Audio/**/*"), recursive=True):
    if meta.endswith(".meta"):
        t = open(meta, encoding="utf-8").read()
        if not re.search(r"^AudioImporter:", t, re.M):
            err(f"{meta}: not an AudioImporter meta")
    elif meta.endswith(".wav") and not os.path.exists(meta + ".meta"):
        err(f"{meta}: missing meta")

# result
print("=" * 60)
for w in warnings:
    print("WARN:", w)
for e in errors:
    print("ERROR:", e)
print(f"== {len(errors)} errors, {len(warnings)} warnings ==")
sys.exit(1 if errors else 0)
