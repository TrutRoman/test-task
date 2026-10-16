export type Track = "Frontend" | "Backend" | "DevOps";

export interface Course {
  title: string;
  track: Track;
  lessons: number;
  weeks: number;
}

export const TRACKS: Track[] = ["Frontend", "Backend", "DevOps"];

// Тестові заглушки каталогу курсів; у базі даних курсів немає
export const COURSES: Course[] = [
  { title: "Курс Тест 1", track: "Frontend", lessons: 24, weeks: 8 },
  { title: "Курс Тест 2", track: "Frontend", lessons: 16, weeks: 6 },
  { title: "Курс Тест 3", track: "Backend", lessons: 20, weeks: 8 },
  { title: "Курс Тест 4", track: "Backend", lessons: 14, weeks: 5 },
  { title: "Курс Тест 5", track: "DevOps", lessons: 8, weeks: 3 },
  { title: "Курс Тест 6", track: "DevOps", lessons: 12, weeks: 4 },
  { title: "Курс Тест 7", track: "Frontend", lessons: 18, weeks: 7 },
  { title: "Курс Тест 8", track: "Backend", lessons: 16, weeks: 6 },
  { title: "Курс Тест 9", track: "DevOps", lessons: 10, weeks: 4 },
];

// Даних про популярність немає, тому «популярні» — перші курси каталогу
const POPULAR_COUNT = 3;

export const POPULAR_COURSES = COURSES.slice(0, POPULAR_COUNT);
