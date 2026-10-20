"use client";

import { Button } from "@/components/ui/button";
import { useState } from "react";

export default function Counter() {
  const [count, setCount] = useState(0);

  const handleClick = () => {
    setCount((prev) => prev + 1);
  };

  return (
    <>
      <span>{count}</span>
      <Button onClick={handleClick}>Увеличить</Button>
    </>
  );
}
