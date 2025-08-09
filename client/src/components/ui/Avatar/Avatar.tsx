import * as React from "react";
import { cn } from "@/lib/utils";

export interface AvatarProps extends React.HTMLAttributes<HTMLDivElement> {
  src?: string;
  alt?: string;
  fallback?: string;
}

export const Avatar: React.FC<AvatarProps> = ({ src, alt, fallback, className, ...props }) => {
  return (
    <div
      className={cn(
        "inline-flex items-center justify-center select-none rounded-full bg-muted text-foreground/80",
        "h-8 w-8 overflow-hidden",
        className
      )}
      {...props}
    >
      {src ? (
        <img src={src} alt={alt} className="h-full w-full object-cover" />
      ) : (
        <span className="text-xs font-medium">{fallback ?? "?"}</span>
      )}
    </div>
  );
};


