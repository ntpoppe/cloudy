export type FileItem = {
    id: string;
    name: string;
    type: "file" | "folder";
    size: number; // bytes
    updatedAt: string; // ISO
    parentId: string | null;
    extension?: string;
    trashed?: boolean;
};