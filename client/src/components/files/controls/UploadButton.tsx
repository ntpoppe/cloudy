import * as React from "react";
import { Button } from "@/components/ui";
import { Upload } from "lucide-react";

export const UploadButton: React.FC<{ onUpload: (files: FileList | null) => void }>
  = ({ onUpload }) => {
  const inputRef = React.useRef<HTMLInputElement | null>(null);
  return (
    <>
      <input ref={inputRef} type="file" multiple className="hidden" onChange={(e) => onUpload(e.target.files)} />
      <Button variant="outline" size="sm" onClick={() => inputRef.current?.click()}>
        <Upload className="h-4 w-4 mr-1" /> Upload
      </Button>
    </>
  );
};


