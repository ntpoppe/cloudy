import * as React from "react";
import { Folder, File as FileIcon } from "lucide-react";
import { formatBytes } from "@/lib/format";
import type { FileItem } from "@/types/FileItem";

export const ListView: React.FC<{
  items: FileItem[];
  selection: Set<string>;
  onOpen: (item: FileItem) => void;
  onRowClick: (e: React.MouseEvent, item: FileItem, index: number) => void;
}> = ({ items, selection, onOpen, onRowClick }) => (
  <div className="rounded-lg border overflow-hidden">
    <div className="grid grid-cols-12 items-center px-3 py-2 text-xs uppercase tracking-wide text-muted-foreground bg-card/40">
      <div className="col-span-6">Name</div>
      <div className="col-span-2">Type</div>
      <div className="col-span-2">Size</div>
      <div className="col-span-2">Updated</div>
    </div>
    <ul className="divide-y">
      {items.map((it, idx) => (
        <li key={it.id}>
          <button
            onDoubleClick={() => onOpen(it)}
            onClick={(e) => onRowClick(e, it, idx)}
            className={`grid grid-cols-12 items-center px-3 py-2 w-full text-left transition-colors ${
              selection.has(it.id) ? "bg-primary/10 ring-2 ring-primary/60" : "hover:bg-accent"
            }`}
          >
            <div className="col-span-6 flex items-center gap-2 truncate">
              {it.type === "folder" ? <Folder className="h-4 w-4 text-primary" /> : <FileIcon className="h-4 w-4" />}
              <span className="truncate">{it.name}</span>
            </div>
            <div className="col-span-2 text-sm text-muted-foreground">{it.type}</div>
            <div className="col-span-2 text-sm text-muted-foreground">{it.type === "file" ? formatBytes(it.size) : "-"}</div>
            <div className="col-span-2 text-sm text-muted-foreground">{new Date(it.updatedAt).toLocaleDateString()}</div>
          </button>
        </li>
      ))}
    </ul>
  </div>
);


