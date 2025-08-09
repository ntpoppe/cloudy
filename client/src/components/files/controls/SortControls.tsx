import * as React from "react";

export const SortControls: React.FC<{
  sortBy: "name" | "size" | "updatedAt";
  sortDir: "asc" | "desc";
  setSortBy: (v: "name" | "size" | "updatedAt") => void;
  setSortDir: (v: "asc" | "desc") => void;
}> = ({ sortBy, sortDir, setSortBy, setSortDir }) => (
  <div className="flex items-center gap-2">
    <span className="text-sm font-medium text-foreground cursor-default select-none">Sort</span>
    <select
      className="h-9 rounded-md border bg-input px-2 text-sm cursor-pointer"
      value={sortBy}
      onChange={(e) => setSortBy(e.target.value as "name" | "size" | "updatedAt")}
      aria-label="Sort by"
    >
      <option value="name">Name</option>
      <option value="size">Size</option>
      <option value="updatedAt">Updated</option>
    </select>
    <select
      className="h-9 rounded-md border bg-input px-2 text-sm cursor-pointer"
      value={sortDir}
      onChange={(e) => setSortDir(e.target.value as "asc" | "desc")}
      aria-label="Sort direction"
    >
      <option value="asc">Asc</option>
      <option value="desc">Desc</option>
    </select>
  </div>
);


