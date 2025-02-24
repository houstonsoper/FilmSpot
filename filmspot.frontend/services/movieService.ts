import Movie from "@/app/interfaces/movie";

const BASE_URL = "https://localhost:7107/Movie";

export async function fetchCurrentShowings(): Promise<Movie[]> {
  try{
    const response: Response = await fetch(BASE_URL);
    if (!response.ok)
      throw new Error("Unable to fetch movies from API");
    
    return await response.json();
  } catch (e){
    console.error(e);
    return [];
  }
}