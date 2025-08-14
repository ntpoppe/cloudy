import * as React from "react";
import { useMemo, useRef, useState } from "react";
import { Button } from "@/components/ui";
import { Card, CardContent } from "@/components/ui";
import { Input } from "@/components/ui";
import { Separator } from "@/components/ui";
import {
  Plus,
  Folder,
  Grid as GridIcon,
  List as ListIcon,
  ChevronLeft,
  Search,
  Trash2,
  Download,
  RefreshCw,
  Pencil,
  MoveRight,
} from "lucide-react";
import { formatBytes } from "@/lib/format";
import { GridView } from "@/components/files/views/GridView";
import { ListView } from "@/components/files/views/ListView";
import { Breadcrumb } from "@/components/breadcrumbs";
import { SortControls } from "@/components/files/controls/SortControls";
import { UploadButton } from "@/components/files/controls/UploadButton";
import { ActivityRow } from "@/components/activity";
import { UserMenu } from "@/components/layout/Topbar";

// --- Types ---
import type { FileItem } from "@/types/FileItem";
import type { ActivityItem } from "@/types/ActivityItem";

// utils moved to @/lib/format

// --- Demo Data ---
const initialItems: FileItem[] = [
  { id: "f1", name: "Projects", type: "folder", size: 0, updatedAt: new Date().toISOString(), parentId: null },
  { id: "f2", name: "Photos", type: "folder", size: 0, updatedAt: new Date().toISOString(), parentId: null },
  { id: "a1", name: "resume.pdf", type: "file", size: 345678, updatedAt: new Date().toISOString(), parentId: null, extension: "pdf" },
  { id: "a2", name: "notes.txt", type: "file", size: 2345, updatedAt: new Date().toISOString(), parentId: null, extension: "txt" },
];

const demoActivities: ActivityItem[] = [
  { id: "ac1", message: "Uploaded file resume.pdf", createdAt: new Date().toISOString() },
  { id: "ac2", message: "Renamed folder Projects → Side Projects", createdAt: new Date().toISOString() },
];

// --- Sidebar ---
const SidebarItem: React.FC<{ icon: React.ReactNode; label: string; active?: boolean; onClick?: () => void }>
  = ({ icon, label, active, onClick }) => (
  <button
    onClick={onClick}
    className={
      `w-full flex items-center gap-2 rounded-md px-2 py-2 text-sm transition-colors ` +
      (active ? `bg-primary/10 text-foreground` : `hover:bg-accent`)
    }
  >
    {icon}
    <span>{label}</span>
  </button>
);

// --- Main Dashboard ---
const Dashboard: React.FC = () => {
  const [items, setItems] = useState<FileItem[]>(initialItems);
  const [path, setPath] = useState<string[]>([]); // array of folder ids
  const [view, setView] = useState<"grid" | "list">("grid");
  const [query, setQuery] = useState("");
  const [sortBy, setSortBy] = useState<"name" | "size" | "updatedAt">("name");
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");
  const [selection, setSelection] = useState<Set<string>>(new Set());
  const [showCreateFolder, setShowCreateFolder] = useState(false);
  const [newFolderName, setNewFolderName] = useState("");
  const [activities, setActivities] = useState<ActivityItem[]>(demoActivities);
  const [usedBytes, setUsedBytes] = useState(345678 + 2345);
  const dropRef = useRef<HTMLDivElement | null>(null);

  const currentFolderId = path[path.length - 1] ?? null;

  const currentItems = useMemo(() => {
    let list = items.filter((it) => it.parentId === currentFolderId && !it.trashed);
    // search
    if (query.trim()) {
      const q = query.toLowerCase();
      list = list.filter((it) => it.name.toLowerCase().includes(q) || (it.extension?.toLowerCase().includes(q) ?? false));
    }
    // sort folders first
    list.sort((a, b) => {
      if (a.type !== b.type) return a.type === "folder" ? -1 : 1;
      const dirMul = sortDir === "asc" ? 1 : -1;
      if (sortBy === "name") return a.name.localeCompare(b.name) * dirMul;
      if (sortBy === "size") return (a.size - b.size) * dirMul;
      return (new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime()) * dirMul;
    });
    return list;
  }, [items, currentFolderId, query, sortBy, sortDir]);

  function openItem(item: FileItem) {
    if (item.type === "folder") {
      setPath((p) => [...p, item.id]);
      setSelection(new Set());
    } else {
      console.log("Open file", item);
    }
  }

  // Basic toggle with Ctrl/Cmd; Shift+click range for ListView only
  function toggleSelect(id: string, multi = false) {
    setSelection((prev) => {
      const next = new Set(prev);
      if (!multi) {
        next.clear();
      }
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  const lastListIndexRef = React.useRef<number | null>(null);

  function handleRangeClick(
    e: React.MouseEvent,
    item: FileItem,
    index: number,
    lastIndexRef: React.RefObject<number | null>
  ) {
    const isCtrlMeta = e.ctrlKey || e.metaKey;
    const isShift = e.shiftKey;

    if (isShift && lastIndexRef.current !== null) {
      const start = Math.min(lastIndexRef.current, index);
      const end = Math.max(lastIndexRef.current, index);
      const next = new Set<string>();
      for (let i = start; i <= end; i++) {
        next.add(currentItems[i].id);
      }
      setSelection(next);
      return;
    }

    // default to toggle with ctrl/cmd or single select
    toggleSelect(item.id, isCtrlMeta);
    lastIndexRef.current = index;
  }

  function handleListRowClick(e: React.MouseEvent, item: FileItem, index: number) {
    handleRangeClick(e, item, index, lastListIndexRef);
  }

  const lastGridIndexRef = React.useRef<number | null>(null);

  function handleGridItemClick(e: React.MouseEvent, item: FileItem, index: number) {
    handleRangeClick(e, item, index, lastGridIndexRef);
  }

  function createFolder(name: string) {
    const id = `fld_${Date.now()}`;
    const folder: FileItem = {
      id,
      name: name || "New folder",
      type: "folder",
      size: 0,
      updatedAt: new Date().toISOString(),
      parentId: currentFolderId,
    };
    setItems((arr) => [folder, ...arr]);
    setActivities((a) => [{ id: `ac_${Date.now()}`, message: `Created folder ${folder.name}`, createdAt: new Date().toISOString() }, ...a]);
  }

  const onUpload = React.useCallback((files: FileList | null) => {
    if (!files || files.length === 0) return;
    let total = 0;
    const newOnes: FileItem[] = [];
    Array.from(files).forEach((f) => {
      const id = `file_${Date.now()}_${Math.random().toString(36).slice(2, 7)}`;
      const ext = f.name.split(".").pop()?.toLowerCase();
      const item: FileItem = {
        id,
        name: f.name,
        type: "file",
        size: f.size,
        updatedAt: new Date().toISOString(),
        parentId: currentFolderId,
        extension: ext,
      };
      newOnes.push(item);
      total += f.size;
    });
    setItems((arr) => [...newOnes, ...arr]);
    setUsedBytes((b) => b + total);
    setActivities((a) => [{ id: `ac_${Date.now()}`, message: `Uploaded ${newOnes.length} file(s)`, createdAt: new Date().toISOString() }, ...a]);
    console.log("Upload", newOnes);
  }, [currentFolderId]);

  function removeSelected(toTrash = true) {
    if (selection.size === 0) return;
    setItems((arr) =>
      arr.map((it) => (selection.has(it.id) ? { ...it, trashed: toTrash } : it))
    );
    setSelection(new Set());
    setActivities((a) => [{ id: `ac_${Date.now()}`, message: `${toTrash ? "Moved to Trash" : "Deleted"} ${selection.size} item(s)`, createdAt: new Date().toISOString() }, ...a]);
  }

  function restoreSelected() {
    if (selection.size === 0) return;
    setItems((arr) => arr.map((it) => (selection.has(it.id) ? { ...it, trashed: false } : it)));
    setSelection(new Set());
    setActivities((a) => [{ id: `ac_${Date.now()}`, message: `Restored ${selection.size} item(s)`, createdAt: new Date().toISOString() }, ...a]);
  }

  function goToRoot() {
    setPath([]);
    setSelection(new Set());
  }

  function goUpOne() {
    if (path.length === 0) return;
    setPath((p) => p.slice(0, -1));
    setSelection(new Set());
  }

  React.useEffect(() => {
    const el = dropRef.current;
    if (!el) return;
    function onPrevent(e: DragEvent) {
      e.preventDefault();
    }
    function onDrop(e: DragEvent) {
      e.preventDefault();
      onUpload(e.dataTransfer?.files ?? null);
    }
    el.addEventListener("dragover", onPrevent);
    el.addEventListener("dragenter", onPrevent);
    el.addEventListener("drop", onDrop);
    return () => {
      el.removeEventListener("dragover", onPrevent);
      el.removeEventListener("dragenter", onPrevent);
      el.removeEventListener("drop", onDrop);
    };
  }, [onUpload]);

  const used = usedBytes;
  const total = 20 * 1024 * 1024 * 1024; // 20 GB demo
  const usedPct = Math.min(100, Math.round((used / total) * 100));

  return (
    <div ref={dropRef} className="h-screen w-screen bg-background text-foreground flex">
      {/* Sidebar */}
      <aside className="hidden md:flex md:w-64 flex-col border-r bg-card/40">
        <div className="h-16 px-4 flex items-center gap-2 border-b">
          <Folder className="h-6 w-6 text-primary" />
          <span className="font-semibold tracking-tight">Cloudy</span>
        </div>
        <nav className="p-3 space-y-1">
          <SidebarItem icon={<Folder className="h-4 w-4" />} label="My Files" active onClick={goToRoot} />
          <SidebarItem icon={<MoveRight className="h-4 w-4" />} label="Shared" />
          <SidebarItem icon={<Trash2 className="h-4 w-4" />} label="Trash" onClick={() => console.log("Open Trash")}/>
          <Separator className="my-3" />
          <SidebarItem icon={<Pencil className="h-4 w-4" />} label="Settings" />
        </nav>
        <div className="mt-auto p-4 space-y-2">
          <div className="text-xs text-muted-foreground">Storage</div>
          <div className="h-2 w-full rounded-full bg-muted overflow-hidden">
            <div className="h-full bg-primary" style={{ width: `${usedPct}%` }} />
          </div>
          <div className="text-xs text-muted-foreground">
            {formatBytes(used)} / {formatBytes(total)} used
          </div>
        </div>
      </aside>

      {/* Main column */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Top bar */}
        <header className="h-16 border-b flex items-center gap-3 px-3 sm:px-4">
          <Breadcrumb
            rootLabel="My Files"
            path={path}
            items={items}
            onRoot={goToRoot}
            onCrumb={(id) => {
              const idx = path.indexOf(id);
              setPath(path.slice(0, idx + 1));
              setSelection(new Set());
            }}
          />
          <div className="ml-auto flex items-center gap-2">
            <div className="relative w-48 sm:w-64">
              <Search className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                className="pl-9 h-9 bg-input"
                aria-label="Search files"
              />
            </div>
            <UploadButton onUpload={onUpload} />
            <Button variant="cloud" size="sm" onClick={() => setShowCreateFolder(true)}>
              <Plus className="h-4 w-4 mr-1" /> New folder
            </Button>
            <div className="ml-2 flex items-center gap-2">
              <Button variant="ghost" size="icon" aria-label="Refresh" onClick={() => console.log("Refresh list")}> 
                <RefreshCw className="h-4 w-4" />
              </Button>
              <UserMenu initials="N" />
            </div>
          </div>
        </header>

        {/* Controls */}
        <div className="flex items-center gap-2 px-3 sm:px-4 py-3 border-b bg-card/30">
          <Button
            variant={view === "grid" ? "cloud" : "ghost"}
            size="sm"
            onClick={() => setView("grid")}
            aria-pressed={view === "grid"}
          >
            <GridIcon className="h-4 w-4 mr-1" /> Grid
          </Button>
          <Button
            variant={view === "list" ? "cloud" : "ghost"}
            size="sm"
            onClick={() => setView("list")}
            aria-pressed={view === "list"}
          >
            <ListIcon className="h-4 w-4 mr-1" /> List
          </Button>
          <Separator orientation="vertical" className="mx-1 h-6" />
          <SortControls sortBy={sortBy} sortDir={sortDir} setSortBy={setSortBy} setSortDir={setSortDir} />
          {path.length > 0 && (
            <>
              <Separator orientation="vertical" className="mx-1 h-6" />
              <Button
                variant="ghost"
                size="sm"
                onClick={goUpOne}
                aria-label="Go back"
                title="Go back"
              >
                <ChevronLeft className="h-4 w-4 mr-1" /> Go back
              </Button>
            </>
          )}
          {selection.size > 0 && (
            <div className="ml-auto flex items-center gap-2">
              <Button size="sm" variant="destructive" onClick={() => removeSelected(true)}>
                <Trash2 className="h-4 w-4 mr-1" /> Trash
              </Button>
              <Button size="sm" variant="outline" onClick={restoreSelected}>Restore</Button>
              <Button size="sm" variant="outline" onClick={() => console.log("Download", Array.from(selection))}>
                <Download className="h-4 w-4 mr-1" /> Download
              </Button>
            </div>
          )}
        </div>

        {/* Content */}
        <main className="flex-1 overflow-auto p-3 sm:p-4">
          {currentItems.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-20 text-center text-muted-foreground">
              <Folder className="h-10 w-10 text-primary/60 mb-3" />
              <div className="text-sm mb-3">No files here yet — Upload files or create a folder</div>
              <div className="flex items-center gap-2">
                <UploadButton onUpload={onUpload} />
                <Button variant="cloud" size="sm" onClick={() => setShowCreateFolder(true)}>
                  <Plus className="h-4 w-4 mr-1" /> New folder
                </Button>
              </div>
            </div>
          ) : view === "grid" ? (
            <GridView items={currentItems} selection={selection} onOpen={openItem} onItemClick={handleGridItemClick} />
          ) : (
            <ListView items={currentItems} selection={selection} onOpen={openItem} onRowClick={handleListRowClick} />
          )}
        </main>

        {/* Activity (affixed, scrollable list, show latest) */}
        <section className="border-t bg-card/30">
          <div className="p-3 sm:p-4">
            <h3 className="text-sm font-medium">Recent activity</h3>
          </div>
          <div className="max-h-56 overflow-auto px-3 sm:px-4 pb-3 pr-1 space-y-2">
            {activities.slice(0, 20).map((a) => (
              <ActivityRow key={a.id} item={a} />
            ))}
          </div>
        </section>
      </div>

      {/* Create folder modal */}
      {showCreateFolder && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md">
            <Card className="shadow-card border-0 bg-card/80 backdrop-blur-sm">
              <CardContent className="pt-6">
                <form
                  onSubmit={(e) => {
                    e.preventDefault();
                    createFolder(newFolderName.trim());
                    setNewFolderName("");
                    setShowCreateFolder(false);
                  }}
                  className="space-y-4"
                >
                  <div>
                    <div className="text-sm font-medium mb-2">New folder</div>
                    <Input
                      placeholder="Folder name"
                      value={newFolderName}
                      onChange={(e) => setNewFolderName(e.target.value)}
                      className="bg-input"
                      autoFocus
                    />
                  </div>
                  <div className="flex justify-end gap-2">
                    <Button type="button" variant="ghost" onClick={() => setShowCreateFolder(false)}>Cancel</Button>
                    <Button type="submit" variant="cloud">Create</Button>
                  </div>
                </form>
              </CardContent>
            </Card>
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;
