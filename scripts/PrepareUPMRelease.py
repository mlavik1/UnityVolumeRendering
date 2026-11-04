import os, sys, json, shutil, subprocess, argparse

# Prepares a UPM package folder for upload (Asset Store Publishing Tools, etc.)
# Usage: python scripts/PrepareUPMRelease.py [--name individual.matias-su-lavik.mingray] [--output Release~]

parser = argparse.ArgumentParser(description="Stage the package for a UPM release.")
parser.add_argument("--name", default="individual.matias-su-lavik.mingray", help="Package name to write to package.json")
parser.add_argument("--output", default="Release~", help="Staging directory (relative to the package root)")
args = parser.parse_args()

package_root = os.path.realpath(os.path.join(os.path.dirname(os.path.realpath(__file__)), os.pardir))

include = [
    "package.json",
    "README.md",
    "CHANGELOG.md",
    "LICENSE.md",
    "CREDITS.md",
    "Third Party Notices.md",
    "DONOTREMOVE-PathSearchFile.txt",
    "Runtime",
    "Editor",
    "ThirdParty",
    "Documentation~",
    "Samples~",
]
exclude = [
    "ThirdParty/SimpleITK",
    "ThirdParty/SimpleITK.meta",
    "ThirdParty/SimpleITK.zip",
    "ThirdParty/SimpleITK.zip.meta",
]

def matches(path, entries):
    return any(path == e or path == e + ".meta" or path.startswith(e + "/") for e in entries)

# Tracked files + untracked files that aren't gitignored
git_files = subprocess.check_output(
    ["git", "ls-files", "--cached", "--others", "--exclude-standard", "-z"],
    cwd=package_root).decode("utf-8").split("\0")
files = sorted(f for f in git_files
               if f and matches(f, include) and not matches(f, exclude)
               and os.path.isfile(os.path.join(package_root, f)))

staging_dir = os.path.join(package_root, args.output, args.name)
if os.path.exists(staging_dir):
    shutil.rmtree(staging_dir)

for f in files:
    dest = os.path.join(staging_dir, f)
    os.makedirs(os.path.dirname(dest), exist_ok=True)
    shutil.copy2(os.path.join(package_root, f), dest)

package_json_path = os.path.join(staging_dir, "package.json")
with open(package_json_path, "r", encoding="utf-8") as file:
    package_json = json.load(file)
old_name = package_json["name"]
package_json["name"] = args.name
with open(package_json_path, "w", encoding="utf-8", newline="\n") as file:
    json.dump(package_json, file, indent=2, ensure_ascii=False)
    file.write("\n")

print("Copied {} files".format(len(files)))
print("Package name: {} -> {}".format(old_name, args.name))
print("Version: {}".format(package_json.get("version")))
print("Staged at: {}".format(staging_dir))
