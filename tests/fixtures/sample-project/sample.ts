export class Sample {
    run(flag: boolean): string[] {
        const first = "a";

        const second = "b";
        const values = [first, second];

        for (const value of values) {
            if (value === "stop") {
                return values;
            }
        }
        if (!flag) {
            return values;
        }
        const summary = values.join(",");
        return values;
    }
}
