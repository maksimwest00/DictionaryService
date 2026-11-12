import { apiClient } from "@/shared/api/axios-instance";
import { PositionListItemResponse } from "./schemas";
import { CursorResult } from "@/shared/api/cursor-result";

export type GetPositionsQuery = {
  cursor?: string;
  pageSize?: number;
  departmentId?: string;
};

export const positionsApi = {
  getPositions: (
    request: GetPositionsQuery = {},
  ): Promise<CursorResult<PositionListItemResponse>> => {
    return apiClient.get<CursorResult<PositionListItemResponse>>("/positions", {
      params: request,
    });
  },
};
