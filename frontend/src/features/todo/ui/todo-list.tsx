"use client";

import { Button } from "@/shared/ui/button";
import { useState } from "react";
import { useTodos } from "../model/use-todos";

export function TodoList() {
  const {
    tasks,
    notCompletedCount,
    addTask,
    toggleTaskIsCompleted,
    deleteTask,
  } = useTodos();
  const [inputValue, setInputValue] = useState("");

  const handleAdd = () => {
    const isAdded = addTask(inputValue);
    if (isAdded) setInputValue("");
  };

  return (
    <div className="flex flex-col gap-2">
      <div className="flex flex-row gap-2">
        <input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          className="border px-2 py-1"
          onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
        <Button onClick={handleAdd}>Добавить задачу</Button>
      </div>

      <span>{notCompletedCount} невыполненных</span>

      {tasks.map((task) => (
        <div key={task.id} className="flex flex-row items-center gap-x-2">
          <span>{task.text}</span>
          <Button onClick={() => toggleTaskIsCompleted(task.id)}>
            {task.isCompleted
              ? "Отметить невыполненной"
              : "Отметить выполненной"}
          </Button>
          <Button onClick={() => deleteTask(task.id)}>Удалить</Button>
        </div>
      ))}
    </div>
  );
}
