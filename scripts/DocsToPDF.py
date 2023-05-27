import os, fnmatch, re, shutil, errno
from distutils.dir_util import copy_tree

def copy_filedir(src, dst):
    print(src)
    print(dst)
    try:
        shutil.copytree(src, dst)
    except OSError as exc: # python >2.5
        if exc.errno in (errno.ENOTDIR, errno.EINVAL):
            shutil.copy(src, dst)
        else: raise
# Delete temp paths
if os.path.exists("DocsTemp"):
    shutil.rmtree("DocsTemp")
if os.path.exists("DocsTempModified"):
    shutil.rmtree("DocsTempModified")

# Create temp paths, and copy documentation to temp folder
copy_tree("Documentation", "DocsTemp")
os.mkdir("DocsTempModified")

# List of all markdown files to process
md_files = []

main_docs_file = "DocsTemp/Documentation.md"
copy_filedir("DocsTemp/Documentation.md", "DocsTempModified/Documentation.md")
md_files.append("Documentation.md")
md_id = 0
with open(main_docs_file) as f:
    s = f.read()
    matches = re.findall(r"\(([^\)]*md)\)", s)
    for file_path in matches:
        file_path = "DocsTemp/" + file_path
        dir_path = os.path.dirname(os.path.abspath(file_path))
        file_name = os.path.basename(os.path.abspath(file_path))
        files_in_dir = os.listdir(dir_path)
        md_files.append(str(md_id) + file_name)
        with open(file_path) as f:
            s = f.read()
        # Replace image references with new modified name
        for sibling in files_in_dir:
            s = s.replace(sibling, str(md_id) + sibling)
        # Replace <img src...> with ![](...)
        s = re.sub(r"<img src=\"([^\"]*)\".*>", r"![](\1)", s)
        with open(file_path, "w") as f:
            f.write(s)
        for file in files_in_dir:
            copy_filedir(os.path.join(dir_path, file), os.path.join("DocsTempModified",  str(md_id) + file))
        md_id += 1


os.chdir("DocsTempModified")
os.system("pandoc {files} -o MANUAL.pdf".format(files=" ".join(md_files)))
shutil.move("MANUAL.pdf", "../MANUAL.pdf")
os.chdir("..")

if os.path.exists("DocsTemp"):
    shutil.rmtree("DocsTemp")
if os.path.exists("DocsTempModified"):
    shutil.rmtree("DocsTempModified")
