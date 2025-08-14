import React, { useEffect, useRef, useState } from "react";
import { Avatar } from "@/components/ui";
import { useAuth } from "@/contexts";
import { cn } from "@/lib/utils";

type Props = {
  initials?: string;
  className?: string;
};

export const UserMenu: React.FC<Props> = ({ initials = "?", className }) => {
  const { logout } = useAuth();
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const buttonRef = useRef<HTMLButtonElement | null>(null);
  const menuId = "user-menu";

  useEffect(() => {
    function handleDocumentClick(e: MouseEvent) {
      if (!containerRef.current) return;
      if (!containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    function handleKeydown(e: KeyboardEvent) {
      if (e.key === "Escape") {
        setOpen(false);
        buttonRef.current?.focus();
      }
    }
    document.addEventListener("mousedown", handleDocumentClick);
    document.addEventListener("keydown", handleKeydown);
    return () => {
      document.removeEventListener("mousedown", handleDocumentClick);
      document.removeEventListener("keydown", handleKeydown);
    };
  }, []);

  return (
    <div className="relative" ref={containerRef}>
      <button
        ref={buttonRef}
        type="button"
        aria-label="User menu"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={menuId}
        onClick={() => setOpen((o) => !o)}
        className={cn("rounded-full focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring", className)}
      >
        <Avatar fallback={initials} className="h-8 w-8 cursor-pointer" />
      </button>
      {open && (
        <div
          id={menuId}
          role="menu"
          aria-orientation="vertical"
          className="absolute right-0 mt-2 w-40 rounded-md border bg-card text-foreground shadow-md z-50 py-1"
        >
          <button
            role="menuitem"
            type="button"
            className="w-full text-left px-3 py-2 text-sm hover:bg-accent cursor-pointer"
            onClick={async () => {
              setOpen(false);
              await logout();
            }}
          >
            Logout
          </button>
        </div>
      )}
    </div>
  );
};

