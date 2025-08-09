import * as React from "react";
// no interactive child button here to avoid nested button issues
import { Folder, File as FileIcon, MoreVertical, Check } from "lucide-react";
import { formatBytes } from "@/lib/format";

import type { FileItem } from "@/types/FileItem";

export const GridView: React.FC<{
  items: FileItem[];
  selection: Set<string>;
  onOpen: (item: FileItem) => void;
  onItemClick: (e: React.MouseEvent, item: FileItem, index: number) => void;
}> = ({ items, selection, onOpen, onItemClick }) => (
  <div className="grid gap-3 grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6">
    {items.map((it, idx) => (
      <button
        key={it.id}
        onDoubleClick={() => onOpen(it)}
        onClick={(e) => onItemClick(e, it, idx)}
        className={`group relative rounded-lg border p-3 text-left transition-colors ${
          selection.has(it.id)
            ? "bg-primary/10 ring-2 ring-primary/60"
            : "bg-card/60 hover:bg-accent"
        }`}
      >
        <div className="flex items-center gap-2">
          {it.type === "folder" ? <Folder className="h-5 w-5 text-primary" /> : <FileIcon className="h-5 w-5" />}
          <div className="truncate font-medium">{it.name}</div>
        </div>
        <div className="mt-2 text-xs text-muted-foreground">
          {it.type === "file" ? formatBytes(it.size) : "Folder"}
        </div>
        <div
          className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none"
          aria-hidden="true"
        >
          {selection.has(it.id) ? (
            <Check className="h-4 w-4 text-primary" />
          ) : (
            <MoreVertical className="h-4 w-4" />
          )}
        </div>
      </button>
    ))}
  </div>
);


