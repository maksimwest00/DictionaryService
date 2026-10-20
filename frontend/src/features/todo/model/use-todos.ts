import { useState } from "react";
import type { Task } from "./types";

export function useTodos(initialState: Task[] = []) {
  const [tasks, setTasks] = useState<Task[]>(initialState);

  const addTask = (text: string) => {
    const trimmed = text.trim();
    if (!trimmed) return false;

    setTasks((prev) => [
      ...prev,
      { id: crypto.randomUUID(), text: trimmed, isCompleted: false },
    ]);
    return true;
  };

  const toggleTaskIsCompleted = (taskId: string) => {
    setTasks((prev) =>
      prev.map((task) =>
        task.id === taskId ? { ...task, isCompleted: !task.isCompleted } : task,
      ),
    );
  };

  const deleteTask = (taskId: string) => {
    setTasks((prev) => prev.filter((task) => task.id !== taskId));
  };

  const notCompletedCount = tasks.filter((task) => !task.isCompleted).length;

  return {
    tasks,
    notCompletedCount,
    addTask,
    toggleTaskIsCompleted,
    deleteTask,
  };
}
