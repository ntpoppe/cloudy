import * as React from "react";
import type { ActivityItem } from "@/types";
import { formatRelativeTime } from "@/lib/format";

export const ActivityRow: React.FC<{ item: ActivityItem }> = ({ item }) => {
  return (
    <div className="flex items-start gap-3 rounded-lg border bg-card/60 p-3">
      <div className="mt-0.5 h-2.5 w-2.5 rounded-full bg-primary shadow-[0_0_0_3px_hsl(var(--primary)/0.15)]" />
      <div className="min-w-0 flex-1">
        <div className="text-sm text-foreground break-words whitespace-pre-wrap">{item.message}</div>
        <div className="text-xs text-muted-foreground">{formatRelativeTime(item.createdAt)}</div>
      </div>
    </div>
  );
};


