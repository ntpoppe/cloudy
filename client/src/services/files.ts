import { http } from './http';
import type { FileItem } from '@/types';

export type CreateIntentPayload = {
  fileName: string;
  contentType: string;
  sizeBytes: number;
};

export type CreateIntentResponse = {
  uploadUrl: string;
  fileId: string;
};

export type FinalizePayload = {
  objectKey: string;
  originalName: string;
  contentType: string;
  sizeBytes: number;
};

// Convert server response to client FileItem
export function mapServerFileToFileItem(
  serverFile: {
    id: number;
    name: string;
    size: number;
    contentType: string;
    uploadedAt: string;
    bucket: string;
    objectKey: string;
  },
  parentId: string | null = null
): FileItem {
  return {
    id: serverFile.id.toString(), // Convert int to string
    name: serverFile.name,
    type: "file" as const,
    size: serverFile.size,
    updatedAt: new Date(serverFile.uploadedAt).toISOString(), // Ensure ISO format
    parentId: parentId,
    extension: serverFile.name.split(".").pop()?.toLowerCase(),
    trashed: false
  };
}

export const fileService = {
  async createUploadIntent(payload: CreateIntentPayload): Promise<CreateIntentResponse> {
    return await http.post<CreateIntentResponse>('/files/intent', payload);
  },

  async uploadToMinio(uploadUrl: string, file: File): Promise<void> {
    await fetch(uploadUrl, {
      method: 'PUT',
      body: file,
      headers: {
        'Content-Type': file.type
      }
    });
  },

  async finalizeUpload(fileId: string, payload: FinalizePayload): Promise<FileItem> {
    const serverFile = await http.post<{
      id: number;
      name: string;
      size: number;
      contentType: string;
      uploadedAt: string;
      bucket: string;
      objectKey: string;
    }>(`/files/${fileId}/finalize`, payload);
    return mapServerFileToFileItem(serverFile);
  },

  async getAll(): Promise<FileItem[]> {
    const serverFiles = await http.get<{
      id: number;
      name: string;
      size: number;
      contentType: string;
      uploadedAt: string;
      bucket: string;
      objectKey: string;
    }[]>('/files');
    return serverFiles.map(file => mapServerFileToFileItem(file));
  },

  async getById(id: string): Promise<FileItem> {
    return await http.get<FileItem>(`/files/${id}`);
  },

  async getDownloadUrl(id: string): Promise<string> {
    return await http.get<string>(`/files/${id}/download-url`);
  },

  async rename(id: string, newName: string): Promise<FileItem> {
    return await http.patch<FileItem>(`/files/${id}/rename`, { newName });
  },

  async delete(id: string): Promise<void> {
    return await http.delete<void>(`/files/${id}`);
  }
};
