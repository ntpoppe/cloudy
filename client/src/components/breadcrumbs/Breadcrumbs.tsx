import * as React from "react";
import type { FileItem } from "@/types";
import { ChevronRight } from "lucide-react";

export const Breadcrumb: React.FC<{
  rootLabel: string;
  path: string[];
  items: FileItem[];
  onRoot: () => void;
  onCrumb: (id: string) => void;
  maxWidthClassName?: string;
}> = ({ rootLabel, path, items, onRoot, onCrumb, maxWidthClassName = "max-w-[45vw]" }) => {
  const nodes = path
    .map((id) => items.find((x) => x.id === id))
    .filter(Boolean) as FileItem[];
  return (
    <div className={`flex items-center gap-1 text-sm overflow-x-auto no-scrollbar ${maxWidthClassName}`}>
      <button className="hover:underline shrink-0" onClick={onRoot}>{rootLabel}</button>
      {nodes.map((n) => (
        <React.Fragment key={n.id}>
          <ChevronRight className="h-4 w-4 text-muted-foreground shrink-0" />
          <button className="hover:underline shrink-0" onClick={() => onCrumb(n.id)}>{n.name}</button>
        </React.Fragment>
      ))}
    </div>
  );
};


