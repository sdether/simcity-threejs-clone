
export class PowerConsumer {
    required = 0;
    supplied = 0;

    constructor(required) {
        this.required = required;
        this.supplied = 0
    }

    copy() {
        return new PowerConsumer(this.required)
    }

    isEqual(other) {
        return this.required === other.required && this.supplied === other.supplied;
    }

}