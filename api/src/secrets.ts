import child_proccess from "node:child_process";
import { promisify } from "node:util";

const exec = promisify(child_proccess.exec);

export async function getBackendSecrets(): Promise<Record<string, string> | undefined>{
  try {
    const { stdout } = await exec("dotnet user-secrets list --project=../backend");
    return stdout
        .split("\n")
        .filter(line => line.includes(" = "))
        .map((line) => line.split(" = "))
        .reduce((secrets, [key, val]) => {
          return { ...secrets, [key]: val.trim() };
        }, {});
  } catch ({ stderr }) {
    console.error("Failed to get user secret via dotnet-cli:", stderr);    
  }

  return undefined;
}
